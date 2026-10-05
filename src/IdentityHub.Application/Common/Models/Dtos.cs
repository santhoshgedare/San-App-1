using System.Text.Json.Serialization;

namespace IdentityHub.Application.Common.Models;

public sealed class UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastLoginAt { get; init; }
    public IReadOnlyCollection<string> Roles { get; init; } = [];
}

public sealed class RoleDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int UserCount { get; init; }
}

public sealed class AuthResultDto
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
    public UserDto User { get; init; } = null!;
}

public sealed class SectionDto
{
    public Guid Id { get; init; }
    public Guid PageId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
}

public sealed class PageDto
{
    public Guid Id { get; init; }
    public Guid ModuleId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyList<SectionDto> Sections { get; init; } = [];
}

public sealed class ModuleDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyList<PageDto> Pages { get; init; } = [];
}

/// <summary>
/// A role's granted section keys, used both to render the settings navigator and to
/// enforce the <c>canRender</c> directive on the Angular client.
/// </summary>
public sealed class RoleAccessDto
{
    public Guid RoleId { get; init; }
    public string RoleName { get; init; } = string.Empty;
    public IReadOnlyList<string> SectionKeys { get; init; } = [];
}

/// <summary>Generic paged result envelope for scroll/offset-based list pagination.</summary>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public bool HasMore => (long)Page * PageSize < TotalCount;
}

/// <summary>An audit trail entry, keyed by entity type + primary key so it can reference any entity.</summary>
public sealed class ActivityLogDto
{
    public Guid Id { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public string EntityId { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string? Details { get; init; }
    public Guid? PerformedByUserId { get; init; }
    public string? PerformedByEmail { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>An approval request, keyed by entity type + primary key so any entity/module can reuse the same workflow.</summary>
public sealed class ApprovalDto
{
    public Guid Id { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public string EntityId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Details { get; init; }
    public string Status { get; init; } = string.Empty;
    public Guid RequestedByUserId { get; init; }
    public string? RequestedByEmail { get; init; }
    public DateTimeOffset RequestedAt { get; init; }
    public Guid? DecidedByUserId { get; init; }
    public string? DecidedByEmail { get; init; }
    public DateTimeOffset? DecidedAt { get; init; }
    public string? DecisionComment { get; init; }

    /// <summary>Snapshotted workflow stage names, in order (empty when no workflow was configured for this entity type).</summary>
    public IReadOnlyList<string> Stages { get; init; } = [];

    /// <summary>Zero-based index of the stage currently awaiting a decision.</summary>
    public int CurrentStageIndex { get; init; }

    /// <summary>1-based revision number for this entity's approval history (increments per new request raised).</summary>
    public int RevisionNumber { get; init; } = 1;

    /// <summary>True only for the latest revision — only one approval per entity may be pending at a time.</summary>
    public bool IsCurrent { get; init; } = true;
    public bool CanDecide { get; init; }

    /// <summary>Per-stage decision history, in stage order.</summary>
    public IReadOnlyList<ApprovalStageDecisionDto> StageDecisions { get; init; } = [];
}

public sealed class ApprovalStageDecisionDto
{
    public int StageIndex { get; init; }
    public string StageName { get; init; } = string.Empty;
    public Guid? AssignedApproverUserId { get; init; }
    public string? AssignedApproverEmail { get; init; }
    public string? Designation { get; init; }
    public string Decision { get; init; } = string.Empty;
    public Guid? DecidedByUserId { get; init; }
    public string? DecidedByEmail { get; init; }
    public DateTimeOffset? DecidedAt { get; init; }
    public string? Comment { get; init; }
}

/// <summary>A single configurable stage of an approval workflow: a name plus the roles eligible to decide it.</summary>
public sealed class ApprovalWorkflowStageDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = [];
    public Guid? DefaultApproverUserId { get; init; }
    public string? DefaultApproverEmail { get; init; }
}

/// <summary>A configurable, multi-stage approval workflow bound to one entity type.</summary>
public sealed class ApprovalWorkflowDto
{
    public Guid Id { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public IReadOnlyList<ApprovalWorkflowStageDto> Stages { get; init; } = [];
}

public sealed class CategoryVariantDefinitionDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public IReadOnlyList<string> Values { get; init; } = [];
    public bool IsRequired { get; init; }
}

public sealed class CategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public string UnitOfMeasurement { get; init; } = string.Empty;
    public IReadOnlyList<CategoryVariantDefinitionDto> VariantDefinitions { get; init; } = [];
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record CategoryVariantDefinitionInput(
    Guid? Id,
    string Name,
    string Type,
    IReadOnlyList<string> Values,
    bool IsRequired);

public sealed record CategoryListQuery(
    string? Search,
    bool? IsActive,
    int Page = 1,
    int PageSize = 20);

public sealed class ItemImageDto
{
    public Guid Id { get; init; }
    public Guid ItemId { get; init; }
    public string Url { get; init; } = string.Empty;
    public string? FileName { get; init; }
    public string? Caption { get; init; }
    public bool IsPrimary { get; init; }
    public int SortOrder { get; init; }
    public DateTimeOffset UploadedAt { get; init; }
}

public sealed record ItemImageInput(
    Guid? Id,
    string Url,
    string? FileName,
    string? Caption,
    bool IsPrimary,
    int SortOrder);

public sealed class ItemDocumentDto
{
    public Guid Id { get; init; }
    public Guid ItemId { get; init; }
    public string Url { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string? DocumentType { get; init; }
    public long FileSizeBytes { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset UploadedAt { get; init; }
}

public sealed record ItemDocumentInput(
    Guid? Id,
    string Url,
    string FileName,
    string? DocumentType,
    long FileSizeBytes,
    string? Description);

public sealed class ItemVariantDto
{
    public Guid Id { get; init; }
    public Guid ItemId { get; init; }
    public string Sku { get; init; } = string.Empty;
    public string? Name { get; init; }
    public string? Barcode { get; init; }
    public string AttributesJson { get; init; } = "{}";
    public decimal Price { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? CostPrice { get; init; }
    public int StockQuantity { get; init; }
    public bool IsActive { get; init; }
}

public sealed record ItemVariantInput(
    Guid? Id,
    string Sku,
    string? Name,
    string? Barcode,
    string AttributesJson,
    decimal Price,
    decimal CostPrice,
    int StockQuantity,
    bool IsActive);

public sealed class ItemDto
{
    public Guid Id { get; init; }
    public Guid? SellerId { get; init; }
    public string SellerName { get; init; } = "SRIVIDIKA";
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Barcode { get; init; }
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string UnitOfMeasurement { get; init; } = string.Empty;
    public decimal Price { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? CostPrice { get; init; }
    public int StockQuantity { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public IReadOnlyList<ItemImageDto> Images { get; init; } = [];
    public IReadOnlyList<ItemDocumentDto> Documents { get; init; } = [];
    public IReadOnlyList<ItemVariantDto> Variants { get; init; } = [];
}

public sealed class OrderItemDto
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public Guid ItemId { get; init; }
    public Guid? ItemVariantId { get; init; }
    public string ItemCode { get; init; } = string.Empty;
    public string ItemName { get; init; } = string.Empty;
    public string? VariantSku { get; init; }
    public string? VariantName { get; init; }
    public string? AttributesJson { get; init; }
    public string? ImageUrl { get; init; }
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public decimal TotalPrice { get; init; }
}

public sealed record OrderItemInput(
    Guid ItemId,
    Guid? ItemVariantId,
    string ItemCode,
    string ItemName,
    string? VariantSku,
    string? VariantName,
    string? AttributesJson,
    string? ImageUrl,
    decimal UnitPrice,
    int Quantity);

public sealed class OrderDto
{
    /// <summary>True when the current user may change this order's status, delivery charge, payment or refund.</summary>
    public bool CanManage { get; set; }
    public Guid? SellerId { get; init; }
    public string SellerName { get; init; } = "SRIVIDIKA";
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public Guid? CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerPhone { get; init; } = string.Empty;
    public string ShippingAddress { get; init; } = string.Empty;
    public string? BillingAddress { get; init; }
    public string? OrderNotes { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public string PaymentStatus { get; init; } = string.Empty;
    public string? PaymentReferenceNumber { get; init; }
    public string? OfflinePaymentNotes { get; init; }
    public decimal? RefundAmount { get; init; }
    public string? RefundReferenceNumber { get; init; }
    public string? RefundNotes { get; init; }
    public DateTimeOffset? RefundedAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? TrackingNumber { get; init; }
    public string? ShippingCarrier { get; init; }
    public decimal SubtotalAmount { get; init; }
    public decimal ShippingFee { get; init; }
    public bool ShippingFeeConfirmed { get; init; }
    public string? ShippingFeeNote { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public IReadOnlyList<OrderItemDto> Items { get; init; } = [];
}

public sealed class ProfitLossReportDto
{
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public int PaidOrderCount { get; init; }
    public int RefundedOrderCount { get; init; }
    public int LegacyRefundCount { get; init; }
    public int MissingCostLineCount { get; init; }
    public decimal GrossSales { get; init; }
    public decimal Refunds { get; init; }
    public decimal NetSales { get; init; }
    public decimal CostOfGoodsSold { get; init; }
    public decimal? GrossProfitOrLoss { get; init; }
    public bool IsComplete { get; init; }
}

public sealed class PaymentSettingsDto
{
    public Guid Id { get; init; }
    public string UpiId { get; init; } = string.Empty;
    public string? PayeeName { get; init; }
    public string? QrCodeImageUrl { get; init; }
    public string? Instructions { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed class AddressDto
{
    public Guid Id { get; init; }
    public string Label { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Line1 { get; init; } = string.Empty;
    public string? Line2 { get; init; }
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string? FormattedAddress { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public bool IsDefault { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record AddressInput(
    string Label,
    string FullName,
    string Phone,
    string Line1,
    string? Line2,
    string City,
    string State,
    string PostalCode,
    string Country,
    string? FormattedAddress,
    double? Latitude,
    double? Longitude,
    bool IsDefault);

public sealed record ItemListQuery(
    string? Search,
    Guid? CategoryId,
    bool? IsActive,
    int Page = 1,
    int PageSize = 20,
    Guid? SellerId = null);

public sealed class SellerDto
{
    public Guid Id { get; init; }
    public Guid? UserId { get; init; }
    public string? UserEmail { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string? Tagline { get; init; }
    public string? Description { get; init; }
    public string? LogoUrl { get; init; }
    public string? ContactEmail { get; init; }
    public string? ContactPhone { get; init; }
    public string? Website { get; init; }
    public string? AddressLine1 { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public int ItemCount { get; init; }
    public string? UpiId { get; init; }
    public string? PayeeName { get; init; }
    public string? QrCodeImageUrl { get; init; }
    public string? BankDetails { get; init; }
    public string? InviteEmail { get; init; }
    /// <summary>None, Pending, Expired or Accepted.</summary>
    public string InviteStatus { get; init; } = "None";
}

public sealed record SellerInviteRequest(string Email);

public sealed class SellerInviteResultDto
{
    public string InviteUrl { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
    public string To { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    /// <summary>Plain-text draft the admin can edit before sending.</summary>
    public string Body { get; init; } = string.Empty;
}

public sealed class EmailLogDto
{
    public Guid Id { get; init; }
    public string ToAddresses { get; init; } = string.Empty;
    public string? CcAddresses { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string? HtmlBody { get; init; }
    public string? TextBody { get; init; }
    public string? Attachments { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Error { get; init; }
    public string Category { get; init; } = string.Empty;
    public string? RelatedEntityType { get; init; }
    public string? RelatedEntityId { get; init; }
    public string? CreatedByEmail { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? SentAt { get; init; }
}

public sealed class SellerInviteInfoDto
{
    public string Email { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
    public string? Tagline { get; init; }
    public string? LogoUrl { get; init; }
}

public sealed record SellerRegistrationInput(
    string FirstName,
    string LastName,
    string Password,
    string PhoneNumber,
    AddressInput Address,
    SellerInput Company);

public sealed record SellerInput(
    string CompanyName,
    string? Tagline,
    string? Description,
    string? LogoUrl,
    string? ContactEmail,
    string? ContactPhone,
    string? Website,
    string? AddressLine1,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    Guid? UserId,
    string? UpiId = null,
    string? PayeeName = null,
    string? BankDetails = null,
    string? QrCodeImageUrl = null);



public sealed class ItemReviewDto
{
    public Guid Id { get; init; }
    public Guid ItemId { get; init; }
    public Guid OrderItemId { get; init; }
    public string ReviewerName { get; init; } = string.Empty;
    public int Rating { get; init; }
    public string? Title { get; init; }
    public string? Comment { get; init; }
    public IReadOnlyList<string> Images { get; init; } = [];
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed class ItemReviewSummaryDto
{
    public double AverageRating { get; init; }
    public int TotalCount { get; init; }
    public IReadOnlyDictionary<int, int> Distribution { get; init; } = new Dictionary<int, int>();
    public IReadOnlyList<ItemReviewDto> Reviews { get; init; } = [];
}
