using System.Security.Claims;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class SellersController(ISellerService sellers, IConfiguration configuration) : ControllerBase
{
    [HttpPost("{id:guid}/invite")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<SellerInviteResultDto>> Invite(Guid id, SellerInviteRequest request, CancellationToken ct)
    {
        var baseUrl = configuration["Client:BaseUrl"] ?? "http://localhost:4200";
        var result = await sellers.CreateInviteAsync(id, request.Email, baseUrl, ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { errors = result.Errors });
    }

    [HttpPost("{id:guid}/invite/send")]
    [Authorize(Roles = Roles.Admin)]
    [RequestSizeLimit(12 * 1024 * 1024)]
    public async Task<IActionResult> SendInviteEmail(Guid id, [FromForm] SellerInviteEmailForm form, CancellationToken ct)
    {
        var attachments = new List<EmailAttachment>();
        foreach (var file in form.Files ?? [])
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, ct);
            attachments.Add(new EmailAttachment(Path.GetFileName(file.FileName), string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType, ms.ToArray()));
        }

        var result = await sellers.SendInviteEmailAsync(id, Split(form.To), Split(form.Cc), form.Subject ?? string.Empty, form.Body ?? string.Empty, attachments, ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    private static string[] Split(string? list)
        => (list ?? string.Empty).Split([',', ';', ' ', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    [HttpGet("invite/{token}")]
    [AllowAnonymous]
    public async Task<ActionResult<SellerInviteInfoDto>> GetInvite(string token, CancellationToken ct)
    {
        var info = await sellers.GetInviteAsync(token, ct);
        return info is null ? NotFound(new { errors = new[] { "This invitation is invalid or has expired." } }) : Ok(info);
    }

    [HttpPost("invite/{token}/accept")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResultDto>> AcceptInvite(string token, SellerRegistrationInput input, CancellationToken ct)
    {
        var result = await sellers.AcceptInviteAsync(token, input, ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Public company card shown as "Fulfilled by" on product pages.</summary>
    [HttpGet("{id:guid}/public")]
    [AllowAnonymous]
    public async Task<ActionResult<SellerDto>> GetPublic(Guid id, CancellationToken ct)
    {
        var seller = await sellers.GetByIdAsync(id, ct);
        if (seller is null) return NotFound();
        return Ok(new SellerDto
        {
            Id = seller.Id,
            CompanyName = seller.CompanyName,
            Tagline = seller.Tagline,
            Description = seller.Description,
            LogoUrl = seller.LogoUrl,
            City = seller.City,
            State = seller.State,
            Country = seller.Country,
            Website = seller.Website
        });
    }

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<IReadOnlyList<SellerDto>>> GetAll(CancellationToken ct)
        => Ok(await sellers.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<SellerDto>> GetById(Guid id, CancellationToken ct)
    {
        var seller = await sellers.GetByIdAsync(id, ct);
        return seller is null ? NotFound() : Ok(seller);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<SellerDto>> Create(SellerInput input, CancellationToken ct)
    {
        var result = await sellers.CreateAsync(input, ct);
        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data)
            : BadRequest(new { errors = result.Errors });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(Guid id, SellerInput input, CancellationToken ct)
    {
        var result = await sellers.UpdateAsync(id, input, true, ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await sellers.DeleteAsync(id, ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    /// <summary>The company profile of the signed-in manager.</summary>
    [HttpGet("mine")]
    [Authorize(Roles = Roles.Manager)]
    public async Task<ActionResult<SellerDto>> GetMine(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var seller = await sellers.GetForUserAsync(userId.Value, ct);
        return seller is null ? NotFound() : Ok(seller);
    }

    [HttpPut("mine")]
    [Authorize(Roles = Roles.Manager)]
    public async Task<IActionResult> UpdateMine(SellerInput input, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var sellerId = await sellers.GetSellerIdForUserAsync(userId.Value, ct);
        if (sellerId is null) return NotFound();

        var result = await sellers.UpdateAsync(sellerId.Value, input, false, ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    private Guid? GetUserId()
        => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out var id) ? id : null;
}

public sealed class SellerInviteEmailForm
{
    public string? To { get; set; }
    public string? Cc { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public List<IFormFile>? Files { get; set; }
}
