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
using IdentityHub.Domain.Constants;
using IdentityHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class OrdersController(ISender sender, IModuleAccessService moduleAccess) : ControllerBase
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

        var result = await sender.Send(new GetOrdersPagedQuery(
            search,
            status,
            paymentStatus,
            customerId,
            page,
            pageSize), ct);

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
        if (!canViewAllOrders && (!currentUserId.HasValue || order.CustomerId != currentUserId))
        {
            return Forbid();
        }

        return Ok(order);
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
        var result = await sender.Send(new UpdateOrderStatusCommand(id, request.Status, request.TrackingNumber, request.ShippingCarrier), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Updates offline payment status (e.g. Paid, Pending, Refunded) with reference number & notes.</summary>
    [HttpPut("{id:guid}/payment-status")]
    [RequireSection("section-orders-payment")]
    public async Task<IActionResult> UpdatePaymentStatus(Guid id, UpdateOrderPaymentStatusRequest request, CancellationToken ct)
    {
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
