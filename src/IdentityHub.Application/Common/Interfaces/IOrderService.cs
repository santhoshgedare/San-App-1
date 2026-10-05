using IdentityHub.Application.Common.Models;
using IdentityHub.Domain.Enums;

namespace IdentityHub.Application.Common.Interfaces;

public interface IOrderService
{
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<OrderDto>> GetPagedAsync(
        string? search,
        OrderStatus? status,
        PaymentStatus? paymentStatus,
        Guid? customerId,
        int page,
        int pageSize,
        CancellationToken ct = default);
    Task<ProfitLossReportDto> GetProfitLossReportAsync(
        DateTimeOffset startInclusive,
        DateTimeOffset endExclusive,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken ct = default);
    Task<Result<OrderDto>> CreateAsync(
        Guid? customerId,
        string customerName,
        string customerEmail,
        string customerPhone,
        string shippingAddress,
        string? billingAddress,
        string? orderNotes,
        PaymentMethod paymentMethod,
        string? paymentReferenceNumber,
        string? offlinePaymentNotes,
        IReadOnlyList<OrderItemInput> items,
        CancellationToken ct = default);
    Task<Result> UpdateStatusAsync(
        Guid id,
        OrderStatus status,
        string? trackingNumber,
        string? shippingCarrier,
        CancellationToken ct = default);
    Task<Result> CancelOwnOrderAsync(Guid id, Guid customerId, CancellationToken ct = default);
    Task<Result> SetShippingFeeAsync(Guid id, decimal shippingFee, string? note, CancellationToken ct = default);
    Task<Result> UpdatePaymentStatusAsync(
        Guid id,
        PaymentStatus paymentStatus,
        string? paymentReferenceNumber,
        string? offlinePaymentNotes,
        CancellationToken ct = default);
    Task<Result> SubmitPaymentDetailsAsync(
        Guid id,
        string paymentReferenceNumber,
        string? offlinePaymentNotes,
        CancellationToken ct = default);
    Task<Result> RecordRefundAsync(
        Guid id,
        decimal refundAmount,
        string refundReferenceNumber,
        string? refundNotes,
        CancellationToken ct = default);
}
