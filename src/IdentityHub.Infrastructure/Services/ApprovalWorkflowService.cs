using System.Text.Json;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Domain.Entities;
using IdentityHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityHub.Infrastructure.Services;

/// <summary>
/// Manages the per-entity-type approval workflow configuration (stages + eligible roles per
/// stage). One active workflow per entity type; saving replaces all of that workflow's stages.
/// </summary>
public sealed class ApprovalWorkflowService(AppDbContext db, Microsoft.AspNetCore.Identity.UserManager<Identity.ApplicationUser> userManager) : IApprovalWorkflowService
{
    public async Task<IReadOnlyList<ApprovalWorkflowDto>> GetAllAsync(CancellationToken ct)
    {
        var workflows = await db.ApprovalWorkflows
            .Include(w => w.Stages)
            .OrderBy(w => w.EntityType)
            .ToListAsync(ct);
        var result = new List<ApprovalWorkflowDto>(workflows.Count);
        foreach (var w in workflows)
        {
            result.Add(await ToDtoAsync(w, ct));
        }
        return result;
    }

    public async Task<ApprovalWorkflowDto?> GetForEntityTypeAsync(string entityType, CancellationToken ct)
    {
        var workflow = await db.ApprovalWorkflows
            .Include(w => w.Stages)
            .FirstOrDefaultAsync(w => w.EntityType == entityType && w.IsActive, ct);
        return workflow is null ? null : await ToDtoAsync(workflow, ct);
    }

    public async Task<Result<ApprovalWorkflowDto>> SaveAsync(SaveApprovalWorkflowRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.EntityType))
        {
            return Result<ApprovalWorkflowDto>.Failure("Entity type is required.");
        }

        if (request.Stages.Count == 0)
        {
            return Result<ApprovalWorkflowDto>.Failure("At least one stage is required.");
        }

        var workflow = request.Id.HasValue
            ? await db.ApprovalWorkflows.Include(w => w.Stages).FirstOrDefaultAsync(w => w.Id == request.Id, ct)
            : await db.ApprovalWorkflows.Include(w => w.Stages).FirstOrDefaultAsync(w => w.EntityType == request.EntityType, ct);

        if (workflow is null)
        {
            workflow = new ApprovalWorkflow { EntityType = request.EntityType };
            db.ApprovalWorkflows.Add(workflow);
        }
        else if (workflow.EntityType != request.EntityType &&
                 await db.ApprovalWorkflows.AnyAsync(w => w.EntityType == request.EntityType && w.Id != workflow.Id, ct))
        {
            return Result<ApprovalWorkflowDto>.Failure("Another workflow already exists for this entity type.");
        }

        workflow.EntityType = request.EntityType;
        workflow.Name = request.Name;
        workflow.IsActive = request.IsActive;

        db.ApprovalWorkflowStages.RemoveRange(workflow.Stages);
        workflow.Stages = request.Stages.Select((stage, index) => new ApprovalWorkflowStage
        {
            WorkflowId = workflow.Id,
            Name = stage.Name,
            SortOrder = index,
            RolesJson = JsonSerializer.Serialize(stage.Roles),
            DefaultApproverUserId = stage.DefaultApproverUserId
        }).ToList();

        await db.SaveChangesAsync(ct);

        return Result<ApprovalWorkflowDto>.Success(await ToDtoAsync(workflow, ct));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct)
    {
        var workflow = await db.ApprovalWorkflows.FirstOrDefaultAsync(w => w.Id == id, ct);
        if (workflow is null)
        {
            return Result.Failure("Workflow not found.");
        }

        db.ApprovalWorkflows.Remove(workflow);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<ApprovalWorkflowDto> ToDtoAsync(ApprovalWorkflow w, CancellationToken ct)
    {
        var stages = new List<ApprovalWorkflowStageDto>();
        foreach (var s in w.Stages.OrderBy(s => s.SortOrder))
        {
            string? defaultApproverEmail = null;
            if (s.DefaultApproverUserId.HasValue)
            {
                var approver = await userManager.Users.FirstOrDefaultAsync(u => u.Id == s.DefaultApproverUserId.Value, ct);
                defaultApproverEmail = approver?.Email;
            }

            stages.Add(new ApprovalWorkflowStageDto
            {
                Id = s.Id,
                Name = s.Name,
                SortOrder = s.SortOrder,
                Roles = JsonSerializer.Deserialize<List<string>>(s.RolesJson) ?? [],
                DefaultApproverUserId = s.DefaultApproverUserId,
                DefaultApproverEmail = defaultApproverEmail
            });
        }

        return new ApprovalWorkflowDto
        {
            Id = w.Id,
            EntityType = w.EntityType,
            Name = w.Name,
            IsActive = w.IsActive,
            Stages = stages
        };
    }
}
