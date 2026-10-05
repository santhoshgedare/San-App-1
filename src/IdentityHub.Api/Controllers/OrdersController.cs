using System.Security.Claims;
using IdentityHub.Api.Contracts;
using IdentityHub.Api.Authorization;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Application.Features.Orders.Commands.CreateOrder;
using IdentityHub.Application.Features.Orders.Commands.RecordOrderRefund;
using IdentityHub.Application.Features.Orders.Commands.SubmitPaymentDetails;
using IdentityHub.Application.Features.Orders.Commands.UpdateOrderStatus;
using IdentityHub.Application.Features.Orders.Commands.UpdatePaymentStatus;
using IdentityHub.Application.Features.Orders.Queries.GetOrderById;
using IdentityHub.Application.Features.Orders.Queries.GetOrders;
using IdentityHub.Application.Features.PaymentSettings.Queries.GetPaymentSettings;
using IdentityHub.Domain.Constants;
using IdentityHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class OrdersController(ISender sender, IModuleAccessService moduleAccess, ISellerService sellers, IOrderService orderStore) : ControllerBase
{
    /// <summary>Lists orders with filtering, search, status, and pagination. Admin/Manager see all, normal user sees own orders.</summary>
    [HttpGet]
    [RequireSection("section-orders-view")]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetOrders(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? paymentStatus,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var canViewAllOrders = User.IsInRole(Roles.Admin) || await HasSectionAsync("section-orders-manage", ct);
        var currentUserId = GetCurrentUserId();
        if (!canViewAllOrders && !currentUserId.HasValue) return Unauthorized();
        Guid? customerId = canViewAllOrders ? null : currentUserId;

        // Sellers (managers) only see orders placed for their own company.
        Guid? sellerId = null;
        if (canViewAllOrders && IsSellerScoped())
        {
            sellerId = await GetOwnSellerIdAsync(ct);
            customerId = currentUserId;
        }

        var result = await sender.Send(new GetOrdersPagedQuery(
            search,
            status,
            paymentStatus,
            customerId,
            page,
            pageSize,
            sellerId), ct);

        return Ok(result);
    }

    /// <summary>Gets a single order by ID.</summary>
    [HttpGet("{id:guid}")]
    [RequireSection("section-orders-view")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken ct)
    {
        var order = await sender.Send(new GetOrderByIdQuery(id), ct);
        if (order is null) return NotFound();

        var canViewAllOrders = User.IsInRole(Roles.Admin) || await HasSectionAsync("section-orders-manage", ct);
        var currentUserId = GetCurrentUserId();
        if (canViewAllOrders && IsSellerScoped() && order.CustomerId != currentUserId && order.SellerId != await GetOwnSellerIdAsync(ct))
        {
            return NotFound();
        }

        if (!canViewAllOrders && (!currentUserId.HasValue || order.CustomerId != currentUserId))
        {
            return Forbid();
        }

        order.CanManage = order.CustomerId != currentUserId && await HasSectionAsync("section-orders-manage", ct) && await CanActOnOrderAsync(id, ct);
        return Ok(order);
    }

    /// <summary>How the buyer pays for this order: the fulfilling seller's UPI/QR, or the platform's for platform-owned orders. Buyer only.</summary>
    [HttpGet("{id:guid}/payment-info")]
    [RequireSection("section-orders-view")]
    public async Task<ActionResult<PaymentSettingsDto>> GetPaymentInfo(Guid id, CancellationToken ct)
    {
        var order = await sender.Send(new GetOrderByIdQuery(id), ct);
        var me = GetCurrentUserId();
        if (order is null || me is null || order.CustomerId != me) return NotFound();
        if (order.Status is nameof(OrderStatus.Pending) or nameof(OrderStatus.Cancelled) || order.PaymentStatus is "Paid" or "Refunded")
        {
            return Conflict(new { errors = new[] { "Payment details are available only after confirmation and until payment is received." } });
        }

        if (order.SellerId is null) return Ok(await sender.Send(new GetPaymentSettingsQuery(), ct));

        var seller = await sellers.GetByIdAsync(order.SellerId.Value, ct);
        if (seller is null || (string.IsNullOrWhiteSpace(seller.UpiId) && string.IsNullOrWhiteSpace(seller.QrCodeImageUrl)))
        {
            return NotFound(new { errors = new[] { "This seller has not set up payment details yet. Please message them in the chat." } });
        }

        return Ok(new PaymentSettingsDto
        {
            Id = seller.Id,
            UpiId = seller.UpiId ?? string.Empty,
            PayeeName = seller.PayeeName ?? seller.CompanyName,
            QrCodeImageUrl = seller.QrCodeImageUrl,
            Instructions = seller.BankDetails
        });
    }

    /// <summary>Places a new order with offline payment workflow.</summary>
    [HttpPost]
    [RequireSection("section-checkout-create")]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderRequest request, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        if (!currentUserId.HasValue) return Unauthorized();

        var itemInputs = request.Items.Select(i => new OrderItemInput(
            i.ItemId,
            i.ItemVariantId,
            i.ItemCode,
            i.ItemName,
            i.VariantSku,
            i.VariantName,
            i.AttributesJson,
            i.ImageUrl,
            i.UnitPrice,
            i.Quantity)).ToList();

        var result = await sender.Send(new CreateOrderCommand(
            currentUserId,
            request.CustomerName,
            request.CustomerEmail,
            request.CustomerPhone,
            request.ShippingAddress,
            request.BillingAddress,
            request.OrderNotes,
            request.PaymentMethod,
            request.PaymentReferenceNumber,
            request.OfflinePaymentNotes,
            itemInputs), ct);

        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data)
            : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Updates order fulfillment status (e.g. Processing, Shipped, Delivered, Cancelled).</summary>
    [HttpPut("{id:guid}/status")]
    [RequireSection("section-orders-manage")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateOrderStatusRequest request, CancellationToken ct)
    {
        if (!await CanActOnOrderAsync(id, ct)) return NotFound();

        var result = await sender.Send(new UpdateOrderStatusCommand(id, request.Status, request.TrackingNumber, request.ShippingCarrier), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Lets a customer cancel their own order while it is still Pending (not yet confirmed).</summary>
    [HttpPost("{id:guid}/cancel")]
    [RequireSection("section-orders-view")]
    public async Task<IActionResult> CancelOwn(Guid id, [FromServices] IOrderService orders, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        if (!currentUserId.HasValue) return Unauthorized();
        var result = await orders.CancelOwnOrderAsync(id, currentUserId.Value, ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Sets the delivery charge for an order before payment and notifies the customer via the order total.</summary>
    [HttpPut("{id:guid}/shipping-fee")]
    [RequireSection("section-orders-manage")]
    public async Task<IActionResult> SetShippingFee(Guid id, SetOrderShippingFeeRequest request, [FromServices] IOrderService orders, CancellationToken ct)
    {
        if (!await CanActOnOrderAsync(id, ct)) return NotFound();

        var result = await orders.SetShippingFeeAsync(id, request.ShippingFee, request.Note, ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Updates offline payment status (e.g. Paid, Pending, Refunded) with reference number & notes.</summary>
    [HttpPut("{id:guid}/payment-status")]
    [RequireSection("section-orders-payment")]
    public async Task<IActionResult> UpdatePaymentStatus(Guid id, UpdateOrderPaymentStatusRequest request, CancellationToken ct)
    {
        if (!await CanActOnOrderAsync(id, ct)) return NotFound();

        var result = await sender.Send(new UpdatePaymentStatusCommand(
            id,
            request.PaymentStatus,
            request.PaymentReferenceNumber,
            request.OfflinePaymentNotes), ct);

        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Records a full refund already processed through the offline payment provider.</summary>
    [HttpPost("{id:guid}/refund")]
    [RequireSection("section-orders-refund")]
    public async Task<IActionResult> RecordRefund(Guid id, RecordOrderRefundRequest request, CancellationToken ct)
    {
        if (!await CanActOnOrderAsync(id, ct)) return NotFound();

        var result = await sender.Send(new RecordOrderRefundCommand(
            id,
            request.RefundAmount,
            request.RefundReferenceNumber,
            request.RefundNotes), ct);

        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Submits payment reference details for verification without changing payment status.</summary>
    [HttpPut("{id:guid}/payment-details")]
    public async Task<IActionResult> SubmitPaymentDetails(Guid id, SubmitOrderPaymentDetailsRequest request, CancellationToken ct)
    {
        var currentOrder = await sender.Send(new GetOrderByIdQuery(id), ct);
        if (currentOrder is null) return NotFound();
        var currentUserId = GetCurrentUserId();
        if (!currentUserId.HasValue) return Unauthorized();
        if (currentOrder.CustomerId != currentUserId) return Forbid();
        if (currentOrder.Status is nameof(OrderStatus.Pending) or nameof(OrderStatus.Cancelled))
        {
            return Conflict(new { error = "Payment can be submitted after the order is confirmed." });
        }
        if (string.Equals(currentOrder.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(currentOrder.PaymentStatus, "Refunded", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new { error = "Payment has already been confirmed." });
        }

        var result = await sender.Send(new SubmitPaymentDetailsCommand(
            id,
            request.PaymentReferenceNumber,
            request.OfflinePaymentNotes), ct);

        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    private bool IsSellerScoped() => !User.IsInRole(Roles.Admin) && User.IsInRole(Roles.Manager);

    private async Task<Guid?> GetOwnSellerIdAsync(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        return userId.HasValue ? await sellers.GetSellerIdForUserAsync(userId.Value, ct) : null;
    }

    private async Task<bool> CanActOnOrderAsync(Guid orderId, CancellationToken ct)
    {
        // Only the fulfilling seller may act on an order; the platform admin acts only on platform-owned (no seller) orders.
        var orderSeller = await orderStore.GetSellerIdAsync(orderId, ct);
        if (orderSeller is null) return User.IsInRole(Roles.Admin);
        return orderSeller == await GetOwnSellerIdAsync(ct);
    }

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var guid) ? guid : null;
    }

    private async Task<bool> HasSectionAsync(string sectionKey, CancellationToken ct)
    {
        var roleNames = User.Claims
            .Where(claim => claim.Type == ClaimTypes.Role || claim.Type == "role")
            .Select(claim => claim.Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var sectionKeys = await moduleAccess.GetSectionKeysForRolesAsync(roleNames, ct);
        return sectionKeys.Contains(sectionKey, StringComparer.Ordinal);
    }
}
