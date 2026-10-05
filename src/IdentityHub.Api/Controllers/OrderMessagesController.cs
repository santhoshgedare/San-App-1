using IdentityHub.Api.Authorization;
using IdentityHub.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityHub.Api.Controllers;

public sealed record SendOrderMessageRequest(string? Body, string? FaqId);

/// <summary>Chat between the buyer and the fulfilling seller on an order.</summary>
[ApiController]
[Route("api/orders/{orderId:guid}/messages")]
[Authorize]
public sealed class OrderMessagesController(IOrderChatService chat) : ControllerBase
{
    [HttpGet]
    [RequireSection("section-orders-view")]
    public async Task<ActionResult<OrderChatDto>> Get(Guid orderId, CancellationToken ct)
    {
        var result = await chat.GetAsync(orderId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [RequireSection("section-orders-view")]
    public async Task<ActionResult<OrderChatDto>> Send(Guid orderId, SendOrderMessageRequest request, CancellationToken ct)
    {
        var error = await chat.SendAsync(orderId, request.Body, request.FaqId, ct);
        if (error is not null) return BadRequest(new { errors = new[] { error } });
        var result = await chat.GetAsync(orderId, ct);
        return result is null ? NotFound() : Ok(result);
    }
}