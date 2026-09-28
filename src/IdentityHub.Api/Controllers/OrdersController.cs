using System.Security.Claims;
using IdentityHub.Api.Contracts;
using IdentityHub.Application.Common.Models;
using IdentityHub.Application.Features.Orders.Commands.CreateOrder;
using IdentityHub.Application.Features.Orders.Commands.UpdateOrderStatus;
using IdentityHub.Application.Features.Orders.Commands.UpdatePaymentStatus;
using IdentityHub.Application.Features.Orders.Queries.GetOrderById;
using IdentityHub.Application.Features.Orders.Queries.GetOrders;
using IdentityHub.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class OrdersController(ISender sender) : ControllerBase
{
    /// <summary>Lists orders with filtering, search, status, and pagination. Admin/Manager see all, normal user sees own orders.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetOrders(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? paymentStatus,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var isPrivileged = User.IsInRole(Roles.Admin) || User.IsInRole(Roles.Manager);
        Guid? customerId = isPrivileged ? null : GetCurrentUserId();

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
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken ct)
    {
        var order = await sender.Send(new GetOrderByIdQuery(id), ct);
        if (order is null) return NotFound();

        var isPrivileged = User.IsInRole(Roles.Admin) || User.IsInRole(Roles.Manager);
        var currentUserId = GetCurrentUserId();
        if (!isPrivileged && order.CustomerId.HasValue && order.CustomerId.Value != currentUserId)
        {
            return Forbid();
        }

        return Ok(order);
    }

    /// <summary>Places a new order with offline payment workflow.</summary>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderRequest request, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();

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
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateOrderStatusRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateOrderStatusCommand(id, request.Status, request.TrackingNumber, request.ShippingCarrier), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Updates offline payment status (e.g. Paid, Pending, Refunded) with reference number & notes.</summary>
    [HttpPut("{id:guid}/payment-status")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<IActionResult> UpdatePaymentStatus(Guid id, UpdateOrderPaymentStatusRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new UpdatePaymentStatusCommand(
            id,
            request.PaymentStatus,
            request.PaymentReferenceNumber,
            request.OfflinePaymentNotes), ct);

        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var guid) ? guid : null;
    }
}
