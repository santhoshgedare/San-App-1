namespace IdentityHub.Api.Contracts;

public sealed record LoginRequest(string Email, string Password);
public sealed record RegisterRequest(string Email, string Password, string FirstName, string LastName, string PhoneNumber, AddressRequest Address);
/// <summary>Requests a password-reset email without revealing whether the address is registered.</summary>
public sealed record ForgotPasswordRequest(string Email);
/// <summary>Completes a password reset using a one-time Identity token.</summary>
public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);
/// <summary>An external sign-in provider available to the client.</summary>
public sealed record ExternalAuthProviderDto(string Provider, string DisplayName, bool Enabled);
public sealed record RefreshTokenRequest(string AccessToken, string RefreshToken);
public sealed record LogoutRequest(string RefreshToken);
public sealed record UpdateUserRequest(string FirstName, string LastName, bool IsActive);
public sealed record CreateUserRequest(string Email, string Password, string FirstName, string LastName, string PhoneNumber, AddressRequest Address, IReadOnlyCollection<string> Roles);
public sealed record AssignRolesRequest(IReadOnlyCollection<string> Roles);
public sealed record CreateRoleRequest(string Name);
public sealed record CreateModuleRequest(string Name, string Key, int SortOrder);
public sealed record CreatePageRequest(Guid ModuleId, string Name, string Url, int SortOrder);
public sealed record CreateSectionRequest(Guid PageId, string Name, string Key, int SortOrder);
public sealed record SetRoleAccessRequest(IReadOnlyCollection<string> SectionKeys);
public sealed record RequestApprovalRequest(string EntityType, string EntityId, string Title, string? Details);
public sealed record ApprovalDecisionRequest(string? Comment);
public sealed record ReassignApprovalStageRequest(int StageIndex, Guid NewApproverUserId);
public sealed record SaveApprovalWorkflowStageDto(string Name, IReadOnlyList<string> Roles, Guid? DefaultApproverUserId);
public sealed record SaveApprovalWorkflowRequestDto(Guid? Id, string EntityType, string Name, bool IsActive, IReadOnlyList<SaveApprovalWorkflowStageDto> Stages);

public sealed record CategoryVariantDefinitionRequest(
    Guid? Id,
    string Name,
    string Type,
    IReadOnlyList<string> Values,
    bool IsRequired);

public sealed record CreateCategoryRequest(
    string Name,
    string? Description,
    bool IsActive,
    string? UnitOfMeasurement,
    IReadOnlyList<CategoryVariantDefinitionRequest>? VariantDefinitions);

public sealed record UpdateCategoryRequest(
    string Name,
    string? Description,
    bool IsActive,
    string? UnitOfMeasurement,
    IReadOnlyList<CategoryVariantDefinitionRequest>? VariantDefinitions);

public sealed record ItemImageRequest(
    Guid? Id,
    string Url,
    string? FileName,
    string? Caption,
    bool IsPrimary,
    int SortOrder);

public sealed record ItemDocumentRequest(
    Guid? Id,
    string Url,
    string FileName,
    string? DocumentType,
    long FileSizeBytes,
    string? Description);

public sealed record ItemVariantRequest(
    Guid? Id,
    string Sku,
    string? Name,
    string? Barcode,
    string AttributesJson,
    decimal Price,
    decimal CostPrice,
    int StockQuantity,
    bool IsActive);

public sealed record CreateItemRequest(
    string Code,
    string Name,
    string? Description,
    string? Barcode,
    Guid CategoryId,
    string? UnitOfMeasurement,
    decimal Price,
    decimal CostPrice,
    int StockQuantity,
    bool IsActive,
    IReadOnlyList<ItemImageRequest>? Images,
    IReadOnlyList<ItemDocumentRequest>? Documents,
    IReadOnlyList<ItemVariantRequest>? Variants);

public sealed record UpdateItemRequest(
    string Code,
    string Name,
    string? Description,
    string? Barcode,
    Guid CategoryId,
    string? UnitOfMeasurement,
    decimal Price,
    decimal CostPrice,
    int StockQuantity,
    bool IsActive,
    IReadOnlyList<ItemImageRequest>? Images,
    IReadOnlyList<ItemDocumentRequest>? Documents,
    IReadOnlyList<ItemVariantRequest>? Variants);

public sealed record OrderItemRequest(
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

public sealed record CreateOrderRequest(
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    string ShippingAddress,
    string? BillingAddress,
    string? OrderNotes,
    string PaymentMethod,
    string? PaymentReferenceNumber,
    string? OfflinePaymentNotes,
    IReadOnlyList<OrderItemRequest> Items);

public sealed record UpdateOrderStatusRequest(
    string Status,
    string? TrackingNumber,
    string? ShippingCarrier);

public sealed record SetOrderShippingFeeRequest(decimal ShippingFee, string? Note);

public sealed record UpdateOrderPaymentStatusRequest(
    string PaymentStatus,
    string? PaymentReferenceNumber,
    string? OfflinePaymentNotes);

public sealed record SubmitOrderPaymentDetailsRequest(
    string PaymentReferenceNumber,
    string? OfflinePaymentNotes);

public sealed record RecordOrderRefundRequest(
    decimal RefundAmount,
    string RefundReferenceNumber,
    string? RefundNotes);

public sealed record UpdatePaymentSettingsRequest(
    string UpiId,
    string? PayeeName,
    string? QrCodeImageUrl,
    string? Instructions);

public sealed record AddressRequest(
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

