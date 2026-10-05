using System.Security.Claims;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityHub.Api.Controllers;

public sealed record CreateReviewRequest(Guid OrderItemId, int Rating, string? Title, string? Comment, List<string>? Images);

[ApiController]
[Route("api")]
[Authorize]
public sealed class ReviewsController(IReviewService reviews) : ControllerBase
{
    /// <summary>Public: rating summary and latest reviews for an item.</summary>
    [HttpGet("items/{itemId:guid}/reviews")]
    [AllowAnonymous]
    public async Task<ActionResult<ItemReviewSummaryDto>> GetForItem(Guid itemId, [FromQuery] int take = 20, CancellationToken ct = default)
        => Ok(await reviews.GetForItemAsync(itemId, take, ct));

    /// <summary>The current user's reviews for the items of one order.</summary>
    [HttpGet("reviews/order/{orderId:guid}")]
    public async Task<ActionResult<IReadOnlyList<ItemReviewDto>>> GetForOrder(Guid orderId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        return userId is null ? Unauthorized() : Ok(await reviews.GetForOrderAsync(orderId, userId.Value, ct));
    }

    /// <summary>Creates a review for a delivered order item owned by the current user.</summary>
    [HttpPost("reviews")]
    public async Task<ActionResult<ItemReviewDto>> Create(CreateReviewRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var result = await reviews.CreateAsync(
            userId.Value, request.OrderItemId, request.Rating, request.Title, request.Comment, request.Images ?? [], ct);

        return result.Succeeded ? Ok(result.Data) : BadRequest(new { errors = result.Errors });
    }

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var guid) ? guid : null;
    }
}