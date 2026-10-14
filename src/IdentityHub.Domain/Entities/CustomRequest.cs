using IdentityHub.Domain.Enums;

namespace IdentityHub.Domain.Entities;

/// <summary>A customer's request for a custom-designed piece, open to quotes from any seller.</summary>
public sealed class CustomRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string RequestNumber { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal? Budget { get; set; }
    public DateOnly? NeededBy { get; set; }

    public CustomRequestStatus Status { get; set; } = CustomRequestStatus.Open;
    public Guid? AwardedThreadId { get; set; }
    public Guid? OrderId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public List<CustomRequestImage> Images { get; set; } = [];
    public List<CustomRequestThread> Threads { get; set; } = [];
}

public sealed class CustomRequestImage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomRequestId { get; set; }
    public CustomRequest? CustomRequest { get; set; }
    /// <summary>Base64 data URI or image URL.</summary>
    public string Url { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>A private conversation (and the seller's latest quote) between the customer and one interested seller.</summary>
public sealed class CustomRequestThread
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomRequestId { get; set; }
    public CustomRequest? CustomRequest { get; set; }
    public Guid SellerId { get; set; }
    public SellerProfile? Seller { get; set; }

    public CustomRequestThreadStatus Status { get; set; } = CustomRequestThreadStatus.Active;

    public decimal? QuoteUnitPrice { get; set; }
    public decimal? QuoteShippingFee { get; set; }
    public int? QuoteLeadTimeDays { get; set; }
    public string? QuoteNote { get; set; }
    public DateTimeOffset? QuotedAt { get; set; }
    public int QuoteRevision { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<CustomRequestMessage> Messages { get; set; } = [];
}

public sealed class CustomRequestMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ThreadId { get; set; }
    public CustomRequestThread? Thread { get; set; }
    public Guid? SenderUserId { get; set; }
    /// <summary>Buyer, Seller or System (quote events).</summary>
    public string SenderRole { get; set; } = "Buyer";
    public string SenderName { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>A production progress update on a custom order, with an optional photo.</summary>
public sealed class OrderProgressUpdate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public CustomOrderStage Stage { get; set; }
    public string? Note { get; set; }
    public string? ImageUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
