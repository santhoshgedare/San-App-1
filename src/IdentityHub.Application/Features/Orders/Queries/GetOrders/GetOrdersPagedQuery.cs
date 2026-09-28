using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Domain.Enums;
using MediatR;

namespace IdentityHub.Application.Features.Orders.Queries.GetOrders;

public sealed record GetOrdersPagedQuery(
    string? Search,
    string? Status,
    string? PaymentStatus,
    Guid? CustomerId,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<OrderDto>>;

public sealed class GetOrdersPagedQueryHandler(IOrderService orderService)
    : IRequestHandler<GetOrdersPagedQuery, PagedResult<OrderDto>>
{
    public async Task<PagedResult<OrderDto>> Handle(GetOrdersPagedQuery request, CancellationToken ct)
    {
        OrderStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<OrderStatus>(request.Status, true, out var s))
        {
            status = s;
        }

        PaymentStatus? paymentStatus = null;
        if (!string.IsNullOrWhiteSpace(request.PaymentStatus) && Enum.TryParse<PaymentStatus>(request.PaymentStatus, true, out var ps))
        {
            paymentStatus = ps;
        }

        return await orderService.GetPagedAsync(
            request.Search,
            status,
            paymentStatus,
            request.CustomerId,
            request.Page,
            request.PageSize,
            ct);
    }
}
