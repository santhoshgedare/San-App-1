using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Domain.Entities;
using IdentityHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityHub.Infrastructure.Services;

public sealed class EmailLogService(AppDbContext db) : IEmailLogService
{
    public async Task<PagedResult<EmailLogDto>> GetPagedAsync(EmailLogQuery query, CancellationToken ct)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var logs = db.EmailLogs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Status)) logs = logs.Where(e => e.Status == query.Status);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            logs = logs.Where(e => e.ToAddresses.Contains(term) || e.Subject.Contains(term) || e.Category.Contains(term));
        }

        var total = await logs.CountAsync(ct);
        var items = await logs.OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(e => new EmailLogDto
            {
                Id = e.Id, ToAddresses = e.ToAddresses, CcAddresses = e.CcAddresses, Subject = e.Subject,
                Attachments = e.Attachments, Status = e.Status, Error = e.Error, Category = e.Category,
                RelatedEntityType = e.RelatedEntityType, RelatedEntityId = e.RelatedEntityId,
                CreatedByEmail = e.CreatedByEmail, CreatedAt = e.CreatedAt, SentAt = e.SentAt
            })
            .ToListAsync(ct);

        return new PagedResult<EmailLogDto> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<EmailLogDto?> GetByIdAsync(Guid id, CancellationToken ct)
        => await db.EmailLogs.AsNoTracking().Where(e => e.Id == id)
            .Select(e => new EmailLogDto
            {
                Id = e.Id, ToAddresses = e.ToAddresses, CcAddresses = e.CcAddresses, Subject = e.Subject,
                HtmlBody = e.HtmlBody, TextBody = e.TextBody, Attachments = e.Attachments, Status = e.Status,
                Error = e.Error, Category = e.Category, RelatedEntityType = e.RelatedEntityType,
                RelatedEntityId = e.RelatedEntityId, CreatedByEmail = e.CreatedByEmail, CreatedAt = e.CreatedAt, SentAt = e.SentAt
            })
            .FirstOrDefaultAsync(ct);
}
