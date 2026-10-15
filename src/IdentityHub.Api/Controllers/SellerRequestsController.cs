using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IdentityHub.Api.Controllers;

public sealed record SellerRequestStatusRequest(string Status, string? AdminNote);

/// <summary>Makers apply to sell from the public site; admins review the requests and invite them.</summary>
[ApiController]
[Route("api/seller-requests")]
public sealed class SellerRequestsController(ISellerRequestService requests) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("seller-request")]
    public async Task<IActionResult> Submit(SellerRequestInput input, CancellationToken ct)
    {
        var result = await requests.SubmitAsync(input, ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<IReadOnlyList<SellerRequestDto>>> GetAll(CancellationToken ct)
        => Ok(await requests.GetAllAsync(ct));

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> UpdateStatus(Guid id, SellerRequestStatusRequest request, CancellationToken ct)
    {
        var result = await requests.UpdateStatusAsync(id, request.Status, request.AdminNote, ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await requests.DeleteAsync(id, ct);
        return result.Succeeded ? NoContent() : NotFound(new { errors = result.Errors });
    }
}
