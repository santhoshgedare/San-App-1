using System.Text.Json;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Domain.Entities;
using IdentityHub.Infrastructure.Identity;
using IdentityHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using IdentityHub.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace IdentityHub.Infrastructure.Services;

/// <summary>
/// Common approval-workflow reader/writer, reusable by any entity/module purely via
/// (entityType, entityId) — mirrors <see cref="ActivityLogService"/>'s generic design.
/// When a workflow is configured for the entity type (via <see cref="IApprovalWorkflowService"/>),
/// new requests snapshot its ordered stages; a decision from any user holding one of the
/// current stage's eligible roles advances (or, on rejection, ends) the request. Entity types
/// without a configured workflow keep the original single Admin/Manager decision behavior.
/// Approve/Reject decisions are also mirrored into the activity log for a unified audit trail.
/// </summary>
public sealed class ApprovalService(
    AppDbContext db,
    ICurrentUserService currentUser,
    IActivityLogService activityLog,
    IApprovalWorkflowService workflowService,
    UserManager<ApplicationUser> userManager) : IApprovalService
{
    public async Task<ApprovalDto> RequestAsync(string entityType, string entityId, string title, string? details, CancellationToken ct)
    {
        // Only one approval per entity may ever be pending/current at a time — reject a new
        // request while a prior one for the same entity is still awaiting a decision.
        var existingCurrent = await db.Approvals
            .Where(a => a.EntityType == entityType && a.EntityId == entityId && a.IsCurrent)
            .FirstOrDefaultAsync(ct);
        if (existingCurrent is not null && existingCurrent.Status == ApprovalStatus.Pending)
        {
            throw new InvalidOperationException("An approval request is already pending for this item.");
        }

        var lastRevision = await db.Approvals
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.RevisionNumber)
            .Select(a => (int?)a.RevisionNumber)
            .FirstOrDefaultAsync(ct) ?? 0;

        if (existingCurrent is not null)
        {
            existingCurrent.IsCurrent = false;
        }

        var workflow = await workflowService.GetForEntityTypeAsync(entityType, ct);

        var approval = new Approval
        {
            EntityType = entityType,
            EntityId = entityId,
            Title = title,
            Details = details,
            RequestedByUserId = currentUser.UserId ?? Guid.Empty,
            RequestedByEmail = currentUser.Email,
            WorkflowId = workflow?.Id,
            CurrentStageIndex = 0,
            RevisionNumber = lastRevision + 1,
            IsCurrent = true
        };

        // Pre-create a stage row for every configured stage up front (status Waiting, except the
        // first which is Pending), each seeded with the workflow stage's default approver, so the
        // full approval-cycle plan (Stage/Approver/Designation/Status) is visible immediately.
        if (workflow is not null)
        {
            for (var i = 0; i < workflow.Stages.Count; i++)
            {
                var stage = workflow.Stages[i];
                approval.StageDecisions.Add(new ApprovalStageDecision
                {
                    ApprovalId = approval.Id,
                    StageIndex = i,
                    StageName = stage.Name,
                    AssignedApproverUserId = stage.DefaultApproverUserId,
                    AssignedApproverEmail = stage.DefaultApproverEmail,
                    Designation = stage.Roles.Count > 0 ? string.Join(", ", stage.Roles) : null,
                    Decision = i == 0 ? ApprovalStatus.Pending : ApprovalStatus.Waiting
                });
            }
        }

        db.Approvals.Add(approval);
        await db.SaveChangesAsync(ct);

        await activityLog.LogAsync(entityType, entityId, "ApprovalRequested", title, ct);

        return await ToDtoAsync(approval, ct);
    }

    public async Task<ApprovalDto> ApproveAsync(Guid approvalId, string? comment, CancellationToken ct)
        => await DecideAsync(approvalId, ApprovalStatus.Approved, comment, ct);

    public async Task<ApprovalDto> RejectAsync(Guid approvalId, string? comment, CancellationToken ct)
        => await DecideAsync(approvalId, ApprovalStatus.Rejected, comment, ct);

    public async Task<ApprovalDto> ReassignStageApproverAsync(Guid approvalId, int stageIndex, Guid newApproverUserId, CancellationToken ct)
    {
        var approval = await db.Approvals.Include(a => a.StageDecisions).FirstOrDefaultAsync(a => a.Id == approvalId, ct)
            ?? throw new KeyNotFoundException("Approval request not found.");

        if (approval.Status != ApprovalStatus.Pending)
        {
            throw new InvalidOperationException("This approval request has already been decided.");
        }

        var stageRow = approval.StageDecisions.FirstOrDefault(d => d.StageIndex == stageIndex)
            ?? throw new KeyNotFoundException("Stage not found on this approval request.");

        if (stageRow.Decision is not (ApprovalStatus.Pending or ApprovalStatus.Waiting))
        {
            throw new InvalidOperationException("Only a pending or waiting stage can be reassigned.");
        }

        // Only Admin, or someone eligible for that stage (i.e. any current/prospective approver), may reassign it.
        var eligibleRoles = await GetStageRolesAsync(approval, stageIndex, ct);
        var orderOwnership = await GetOrderOwnershipAsync(approval, ct);
        if (orderOwnership is not null)
        {
            if (!await CanCurrentUserDecideAsync(approval, ct))
            {
                throw new UnauthorizedAccessException("Only the seller fulfilling this order can reassign its approval.");
            }
        }
        else if (!currentUser.IsInRole(Domain.Constants.Roles.Admin) && !eligibleRoles.Any(currentUser.IsInRole))
        {
            throw new UnauthorizedAccessException("You are not eligible to reassign this stage's approver.");
        }

        var newApprover = await userManager.Users.FirstOrDefaultAsync(u => u.Id == newApproverUserId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        if (eligibleRoles.Count > 0)
        {
            var newApproverRoles = await userManager.GetRolesAsync(newApprover);
            if (!eligibleRoles.Any(newApproverRoles.Contains))
            {
                throw new InvalidOperationException("The selected user does not hold a role eligible for this stage.");
            }
        }

        stageRow.AssignedApproverUserId = newApprover.Id;
        stageRow.AssignedApproverEmail = newApprover.Email;

        await db.SaveChangesAsync(ct);
        await activityLog.LogAsync(approval.EntityType, approval.EntityId, "ApprovalStageReassigned", $"{stageRow.StageName} \u2192 {newApprover.Email}", ct);

        return await ToDtoAsync(approval, ct);
    }

    private async Task<ApprovalDto> DecideAsync(Guid approvalId, ApprovalStatus decision, string? comment, CancellationToken ct)
    {
        var approval = await db.Approvals.Include(a => a.StageDecisions).FirstOrDefaultAsync(a => a.Id == approvalId, ct)
            ?? throw new KeyNotFoundException("Approval request not found.");

        if (approval.Status != ApprovalStatus.Pending)
        {
            throw new InvalidOperationException("This approval request has already been decided.");
        }

        var stageCount = await GetStageCountAsync(approval, ct);
        var stageRow = approval.StageDecisions.FirstOrDefault(d => d.StageIndex == approval.CurrentStageIndex);
        var currentStageName = stageRow?.StageName ?? "Decision";

        if (!await CanCurrentUserDecideAsync(approval, ct))
        {
            throw new UnauthorizedAccessException("You are not eligible to decide the current approval stage.");
        }

        if (stageRow is null)
        {
            // Legacy, workflow-less entity type: no pre-created stage rows exist, create one now.
            stageRow = new ApprovalStageDecision
            {
                ApprovalId = approval.Id,
                StageIndex = approval.CurrentStageIndex,
                StageName = currentStageName
            };
            approval.StageDecisions.Add(stageRow);
            db.Entry(stageRow).State = EntityState.Added;
        }

        stageRow.Decision = decision;
        stageRow.DecidedByUserId = currentUser.UserId;
        stageRow.DecidedByEmail = currentUser.Email;
        stageRow.DecidedAt = DateTimeOffset.UtcNow;
        stageRow.Comment = comment;

        if (decision == ApprovalStatus.Rejected)
        {
            approval.Status = ApprovalStatus.Rejected;
            approval.DecidedByUserId = currentUser.UserId;
            approval.DecidedByEmail = currentUser.Email;
            approval.DecidedAt = DateTimeOffset.UtcNow;
            approval.DecisionComment = comment;
        }
        else if (approval.CurrentStageIndex + 1 >= Math.Max(stageCount, 1))
        {
            // Final stage approved (or no workflow configured, i.e. single-stage) — request is fully approved.
            approval.Status = ApprovalStatus.Approved;
            approval.DecidedByUserId = currentUser.UserId;
            approval.DecidedByEmail = currentUser.Email;
            approval.DecidedAt = DateTimeOffset.UtcNow;
            approval.DecisionComment = comment;
        }
        else
        {
            approval.CurrentStageIndex++;
            var nextStageRow = approval.StageDecisions.FirstOrDefault(d => d.StageIndex == approval.CurrentStageIndex);
            if (nextStageRow is not null)
            {
                nextStageRow.Decision = ApprovalStatus.Pending;
            }
        }

        await db.SaveChangesAsync(ct);

        var action = decision == ApprovalStatus.Rejected ? "ApprovalRejected" : approval.Status == ApprovalStatus.Approved ? "ApprovalApproved" : "ApprovalStageAdvanced";
        var detail = comment ?? $"{currentStageName}: {decision}";
        await activityLog.LogAsync(approval.EntityType, approval.EntityId, action, detail, ct);

        return await ToDtoAsync(approval, ct);
    }

    /// <summary>True when the current user holds one of the current stage's eligible roles (or is Admin/Manager for legacy, workflow-less entity types).</summary>
    private async Task<bool> CanCurrentUserDecideAsync(Approval approval, CancellationToken ct)
    {
        var ownership = await GetOrderOwnershipAsync(approval, ct);
        if (ownership is not null)
        {
            return ownership.Value.SellerUserId is null
                ? currentUser.IsInRole(Domain.Constants.Roles.Admin)
                : ownership.Value.SellerUserId == currentUser.UserId;
        }

        if (approval.WorkflowId is null)
        {
            return currentUser.IsInRole(Domain.Constants.Roles.Admin) || currentUser.IsInRole(Domain.Constants.Roles.Manager);
        }

        var eligibleRoles = await GetStageRolesAsync(approval, approval.CurrentStageIndex, ct);
        if (eligibleRoles.Count == 0)
        {
            return currentUser.IsInRole(Domain.Constants.Roles.Admin);
        }

        return eligibleRoles.Any(currentUser.IsInRole) || currentUser.IsInRole(Domain.Constants.Roles.Admin);
    }

    /// <summary>For order approvals returns the fulfilling seller's login user (null = platform-owned order); null for other entity types.</summary>
    private async Task<(Guid? SellerUserId, Guid? SellerId)?> GetOrderOwnershipAsync(Approval approval, CancellationToken ct)
    {
        if (approval.EntityType != EntityTypes.Order || !Guid.TryParse(approval.EntityId, out var orderId))
        {
            return null;
        }

        var sellerId = await db.Orders.Where(o => o.Id == orderId).Select(o => o.SellerId).FirstOrDefaultAsync(ct);
        if (sellerId is null)
        {
            return (null, null);
        }

        var sellerUserId = await db.SellerProfiles.Where(s => s.Id == sellerId).Select(s => s.UserId).FirstOrDefaultAsync(ct);
        return (sellerUserId ?? Guid.Empty, sellerId);
    }

    private async Task<List<string>> GetStageRolesAsync(Approval approval, int stageIndex, CancellationToken ct)
    {
        if (approval.WorkflowId is null)
        {
            return [];
        }

        var workflow = await db.ApprovalWorkflows.Include(w => w.Stages).FirstOrDefaultAsync(w => w.Id == approval.WorkflowId, ct);
        var stage = workflow?.Stages.OrderBy(s => s.SortOrder).ElementAtOrDefault(stageIndex);
        return stage is null ? [] : JsonSerializer.Deserialize<List<string>>(stage.RolesJson) ?? [];
    }

    private async Task<int> GetStageCountAsync(Approval approval, CancellationToken ct)
    {
        if (approval.WorkflowId is null)
        {
            return 0;
        }

        return await db.ApprovalWorkflowStages.CountAsync(s => s.WorkflowId == approval.WorkflowId, ct);
    }

    public async Task<IReadOnlyList<ApprovalDto>> GetForEntityAsync(string entityType, string entityId, CancellationToken ct)
    {
        var approvals = await db.Approvals
            .Include(a => a.StageDecisions)
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.RequestedAt)
            .ToListAsync(ct);

        var result = new List<ApprovalDto>(approvals.Count);
        foreach (var approval in approvals)
        {
            result.Add(await ToDtoAsync(approval, ct));
        }
        return result;
    }

    public async Task<PagedResult<ApprovalDto>> GetPagedAsync(ApprovalQuery query, CancellationToken ct)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var approvalsQuery = db.Approvals.Include(a => a.StageDecisions).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.EntityType))
        {
            approvalsQuery = approvalsQuery.Where(a => a.EntityType == query.EntityType);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityId))
        {
            approvalsQuery = approvalsQuery.Where(a => a.EntityId == query.EntityId);
        }

        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<ApprovalStatus>(query.Status, true, out var status))
        {
            approvalsQuery = approvalsQuery.Where(a => a.Status == status);
        }

        if (!currentUser.IsInRole(Domain.Constants.Roles.Admin) && currentUser.UserId is { } me)
        {
            var mySellerId = await db.SellerProfiles.Where(s => s.UserId == me).Select(s => (Guid?)s.Id).FirstOrDefaultAsync(ct);
            var orderType = EntityTypes.Order;
            var myOrderIds = db.Orders.Where(o => o.SellerId == mySellerId && mySellerId != null).Select(o => o.Id.ToString());
            approvalsQuery = approvalsQuery.Where(a => a.EntityType != orderType || myOrderIds.Contains(a.EntityId));
        }

        var totalCount = await approvalsQuery.CountAsync(ct);
        var pageOfApprovals = await approvalsQuery
            .OrderByDescending(a => a.RequestedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = new List<ApprovalDto>(pageOfApprovals.Count);
        foreach (var approval in pageOfApprovals)
        {
            items.Add(await ToDtoAsync(approval, ct));
        }

        return new PagedResult<ApprovalDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private async Task<ApprovalDto> ToDtoAsync(Approval a, CancellationToken ct) => new()
    {
        Id = a.Id,
        EntityType = a.EntityType,
        EntityId = a.EntityId,
        Title = a.Title,
        Details = a.Details,
        Status = a.Status.ToString(),
        RequestedByUserId = a.RequestedByUserId,
        RequestedByEmail = a.RequestedByEmail,
        RequestedAt = a.RequestedAt,
        DecidedByUserId = a.DecidedByUserId,
        DecidedByEmail = a.DecidedByEmail,
        DecidedAt = a.DecidedAt,
        DecisionComment = a.DecisionComment,
        Stages = a.StageDecisions.OrderBy(d => d.StageIndex).Select(d => d.StageName).ToList(),
        CurrentStageIndex = a.CurrentStageIndex,
        RevisionNumber = a.RevisionNumber,
        IsCurrent = a.IsCurrent,
        CanDecide = a.Status == ApprovalStatus.Pending && await CanCurrentUserDecideAsync(a, ct),
        StageDecisions = a.StageDecisions
            .OrderBy(d => d.StageIndex)
            .Select(d => new ApprovalStageDecisionDto
            {
                StageIndex = d.StageIndex,
                StageName = d.StageName,
                AssignedApproverUserId = d.AssignedApproverUserId,
                AssignedApproverEmail = d.AssignedApproverEmail,
                Designation = d.Designation,
                Decision = d.Decision.ToString(),
                DecidedByUserId = d.DecidedByUserId,
                DecidedByEmail = d.DecidedByEmail,
                DecidedAt = d.DecidedAt,
                Comment = d.Comment
            })
            .ToList()
    };
}
