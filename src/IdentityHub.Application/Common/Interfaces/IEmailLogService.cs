using IdentityHub.Application.Common.Models;

namespace IdentityHub.Application.Common.Interfaces;

public sealed record EmailAttachment(string FileName, string ContentType, byte[] Content);

public sealed class EmailMessage
{
    public IReadOnlyList<string> To { get; init; } = [];
    public IReadOnlyList<string> Cc { get; init; } = [];
    public string Subject { get; init; } = string.Empty;
    public string HtmlBody { get; init; } = string.Empty;
    public string TextBody { get; init; } = string.Empty;
    public IReadOnlyList<EmailAttachment> Attachments { get; init; } = [];
    public string Category { get; init; } = "General";
    public string? RelatedEntityType { get; init; }
    public string? RelatedEntityId { get; init; }
}

public interface IEmailLogService
{
    Task<PagedResult<EmailLogDto>> GetPagedAsync(EmailLogQuery query, CancellationToken ct);
    Task<EmailLogDto?> GetByIdAsync(Guid id, CancellationToken ct);
}

public sealed record EmailLogQuery
{
    public string? Status { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
