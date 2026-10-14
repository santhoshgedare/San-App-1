using IdentityHub.Application.Common.Models;

namespace IdentityHub.Application.Common.Interfaces;

public sealed record CustomRequestImageInput(string Url, string? FileName);

public sealed record CreateCustomRequestInput(
    string Title,
    string Description,
    int Quantity,
    decimal? Budget,
    DateOnly? NeededBy,
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    IReadOnlyList<CustomRequestImageInput> Images);

public sealed record CustomQuoteInput(decimal UnitPrice, decimal? ShippingFee, int? LeadTimeDays, string? Note);

public sealed record AwardCustomRequestInput(string ShippingAddress, string? OrderNotes);

public sealed class CustomRequestMessageDto
{
    public Guid Id { get; init; }
    public string SenderRole { get; init; } = string.Empty;
    public string SenderName { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public bool IsMine { get; init; }
}

public sealed class CustomRequestThreadDto
{
    public Guid Id { get; init; }
    public Guid SellerId { get; init; }
    public string SellerName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal? QuoteUnitPrice { get; init; }
    public decimal? QuoteShippingFee { get; init; }
    public int? QuoteLeadTimeDays { get; init; }
    public string? QuoteNote { get; init; }
    public DateTimeOffset? QuotedAt { get; init; }
    public int QuoteRevision { get; init; }
    /// <summary>Unit price x quantity plus shipping, when a quote exists.</summary>
    public decimal? QuoteTotal { get; init; }
    public bool IsMine { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public IReadOnlyList<CustomRequestMessageDto> Messages { get; init; } = [];
}

public sealed class CustomRequestSummaryDto
{
    public Guid Id { get; init; }
    public string RequestNumber { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal? Budget { get; init; }
    public DateOnly? NeededBy { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
    public int InterestedCount { get; init; }
    public int QuoteCount { get; init; }
    public Guid? MyThreadId { get; init; }
    public Guid? OrderId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed class CustomRequestDetailDto
{
    public Guid Id { get; init; }
    public string RequestNumber { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal? Budget { get; init; }
    public DateOnly? NeededBy { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public Guid? OrderId { get; init; }
    public Guid? AwardedThreadId { get; init; }
    /// <summary>Customer, Seller or Observer.</summary>
    public string MyRole { get; init; } = "Observer";
    public bool CanExpressInterest { get; init; }
    public IReadOnlyList<string> Images { get; init; } = [];
    /// <summary>Customers and observers see every thread; a seller sees only their own.</summary>
    public IReadOnlyList<CustomRequestThreadDto> Threads { get; init; } = [];
}

public sealed class OrderProgressUpdateDto
{
    public Guid Id { get; init; }
    public string Stage { get; init; } = string.Empty;
    public string? Note { get; init; }
    public string? ImageUrl { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Custom design requests: a customer posts photos and a description, sellers show interest and quote in
/// private threads, and the customer awards one quote, which creates a regular order.
/// </summary>
public interface ICustomRequestService
{
    Task<Result<CustomRequestDetailDto>> CreateAsync(CreateCustomRequestInput input, CancellationToken ct);

    /// <summary>Own requests for customers; open requests plus joined ones for sellers; everything for admins.</summary>
    Task<PagedResult<CustomRequestSummaryDto>> GetPagedAsync(string? scope, string? status, int page, int pageSize, CancellationToken ct);

    /// <summary>Null when the request does not exist or the current user may not see it.</summary>
    Task<CustomRequestDetailDto?> GetAsync(Guid id, CancellationToken ct);

    Task<Result<CustomRequestDetailDto>> ExpressInterestAsync(Guid id, string? message, CancellationToken ct);
    Task<Result<CustomRequestDetailDto>> SendMessageAsync(Guid id, Guid threadId, string body, CancellationToken ct);
    Task<Result<CustomRequestDetailDto>> SubmitQuoteAsync(Guid id, Guid threadId, CustomQuoteInput quote, CancellationToken ct);
    Task<Result<CustomRequestDetailDto>> WithdrawAsync(Guid id, Guid threadId, CancellationToken ct);
    Task<Result<CustomRequestDetailDto>> CancelAsync(Guid id, CancellationToken ct);

    /// <summary>Customer accepts a thread's quote; creates the order and the custom item and returns the order id.</summary>
    Task<Result<Guid>> AwardAsync(Guid id, Guid threadId, AwardCustomRequestInput input, CancellationToken ct);

    /// <summary>Seller of a custom order records a production stage.</summary>
    Task<Result> AddProgressAsync(Guid orderId, string stage, string? note, string? imageUrl, CancellationToken ct);
}
