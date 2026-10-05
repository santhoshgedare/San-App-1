using System.Security.Claims;
using IdentityHub.Api.Contracts;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityHub.Api.Controllers;

/// <summary>
/// Manages the current user's saved addresses (multiple per user, one default), used during
/// registration, the "My Addresses" profile page, and quick-select at checkout.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AddressesController(IAddressService addressService) : ControllerBase
{
    /// <summary>Lists the current user's saved addresses (default first, newest first).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AddressDto>>> GetMine(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        return Ok(await addressService.GetForUserAsync(userId.Value, ct));
    }

    /// <summary>Gets a single saved address by ID (must belong to the current user).</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AddressDto>> GetById(Guid id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var address = await addressService.GetByIdAsync(userId.Value, id, ct);
        return address is null ? NotFound() : Ok(address);
    }

    /// <summary>Adds a new address for the current user.</summary>
    [HttpPost]
    public async Task<ActionResult<AddressDto>> Create(AddressRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var result = await addressService.CreateAsync(userId.Value, ToInput(request), ct);
        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data)
            : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Updates an existing address belonging to the current user.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AddressDto>> Update(Guid id, AddressRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var result = await addressService.UpdateAsync(userId.Value, id, ToInput(request), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Marks the given address as the user's default (unsetting any previous default).</summary>
    [HttpPut("{id:guid}/set-default")]
    public async Task<IActionResult> SetDefault(Guid id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var result = await addressService.SetDefaultAsync(userId.Value, id, ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Soft-deletes an address belonging to the current user.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var result = await addressService.DeleteAsync(userId.Value, id, ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    internal static AddressInput ToInput(AddressRequest? r) => r is null ? null! : new(
        r.Label,
        r.FullName,
        r.Phone,
        r.Line1,
        r.Line2,
        r.City,
        r.State,
        r.PostalCode,
        r.Country,
        r.FormattedAddress,
        r.Latitude,
        r.Longitude,
        r.IsDefault);

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var guid) ? guid : null;
    }
}
