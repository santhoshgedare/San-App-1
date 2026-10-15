using System.Net;
using System.Net.Mail;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Domain.Constants;
using IdentityHub.Domain.Entities;
using IdentityHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IdentityHub.Infrastructure.Services;

public sealed class SellerRequestService(
    AppDbContext db,
    IEmailSender emailSender,
    IConfiguration configuration,
    ILogger<SellerRequestService> logger) : ISellerRequestService
{
    private static readonly string[] Statuses = ["New", "Contacted", "Invited", "Declined"];

    public async Task<Result> SubmitAsync(SellerRequestInput input, CancellationToken ct)
    {
        var name = Clean(input.Name, 150);
        var email = Clean(input.Email, 256);
        var phone = Clean(input.Phone, 32);
        if (name is null || email is null || phone is null) return Result.Failure("Name, email and phone are required.");
        if (!MailAddress.TryCreate(email, out _)) return Result.Failure("Enter a valid email address.");

        var duplicate = await db.SellerRequests.AnyAsync(r => r.Email == email && (r.Status == "New" || r.Status == "Contacted"), ct);
        if (duplicate) return Result.Failure("We already have a request from this email. We will contact you soon.");

        var request = new SellerRequest
        {
            Name = name,
            Email = email,
            Phone = phone,
            BusinessName = Clean(input.BusinessName, 200),
            City = Clean(input.City, 100),
            CraftType = Clean(input.CraftType, 150),
            Website = Clean(input.Website, 300),
            Message = Clean(input.Message, 2000),
        };
        db.SellerRequests.Add(request);
        await db.SaveChangesAsync(ct);
        await NotifyAdminsAsync(request, ct);
        return Result.Success();
    }

    private async Task NotifyAdminsAsync(SellerRequest r, CancellationToken ct)
    {
        try
        {
            var admins = await (from ur in db.UserRoles
                                join role in db.Roles on ur.RoleId equals role.Id
                                join u in db.Users on ur.UserId equals u.Id
                                where role.Name == Roles.Admin && u.Email != null
                                select u.Email!).ToListAsync(ct);
            if (admins.Count == 0) return;

            var baseUrl = (configuration["Client:BaseUrl"] ?? "http://localhost:4200").TrimEnd('/');
            string E(string? v) => WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(v) ? "-" : v);
            var rows = new (string Label, string? Value)[]
            {
                ("Name", r.Name), ("Email", r.Email), ("Phone", r.Phone), ("Business", r.BusinessName),
                ("City", r.City), ("Craft", r.CraftType), ("Website", r.Website), ("Message", r.Message),
            };
            var html = "<div style=\"font-family:Georgia,serif;max-width:560px;margin:auto;padding:24px;background:#fbf6ec;color:#3a2a22\">" +
                       "<h2 style=\"color:#6b1f2a;margin:0 0 8px\">SRIVIDIKA</h2><h3 style=\"margin:0 0 12px\">New seller request</h3>" +
                       string.Concat(rows.Select(x => $"<p style=\"margin:4px 0\"><b>{x.Label}:</b> {E(x.Value)}</p>")) +
                       $"<p style=\"margin-top:16px\"><a href=\"{WebUtility.HtmlEncode(baseUrl)}/sellers\" style=\"background:#6b1f2a;color:#fff;padding:10px 18px;border-radius:8px;text-decoration:none\">Review requests</a></p></div>";
            var text = "New seller request\n" + string.Join("\n", rows.Select(x => $"{x.Label}: {x.Value}")) + $"\n\nReview: {baseUrl}/sellers";

            await emailSender.SendAsync(new EmailMessage
            {
                To = admins.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                Subject = $"New seller request from {r.Name}",
                HtmlBody = html,
                TextBody = text,
                Category = "SellerRequest",
                RelatedEntityType = "SellerRequest",
                RelatedEntityId = r.Id.ToString(),
            }, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Could not email admins about seller request {RequestId}.", r.Id);
        }
    }

    public async Task<IReadOnlyList<SellerRequestDto>> GetAllAsync(CancellationToken ct)
        => await db.SellerRequests.AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new SellerRequestDto(r.Id, r.Name, r.Email, r.Phone, r.BusinessName, r.City, r.CraftType, r.Website, r.Message, r.Status, r.AdminNote, r.CreatedAt))
            .ToListAsync(ct);

    public async Task<Result> UpdateStatusAsync(Guid id, string status, string? adminNote, CancellationToken ct)
    {
        if (!Statuses.Contains(status)) return Result.Failure("Unknown status.");
        var request = await db.SellerRequests.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (request is null) return Result.Failure("Request not found.");
        request.Status = status;
        request.AdminNote = Clean(adminNote, 1000) ?? request.AdminNote;
        request.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct)
    {
        var request = await db.SellerRequests.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (request is null) return Result.Failure("Request not found.");
        db.SellerRequests.Remove(request);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    private static string? Clean(string? value, int max)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed)) return null;
        return trimmed.Length > max ? trimmed[..max] : trimmed;
    }
}
