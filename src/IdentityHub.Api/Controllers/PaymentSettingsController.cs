using IdentityHub.Api.Contracts;
using IdentityHub.Api.Authorization;
using IdentityHub.Application.Common.Models;
using IdentityHub.Application.Features.PaymentSettings.Commands.UpdatePaymentSettings;
using IdentityHub.Application.Features.PaymentSettings.Queries.GetPaymentSettings;
using IdentityHub.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class PaymentSettingsController(ISender sender) : ControllerBase
{
    /// <summary>Gets the store's UPI QR payment settings, shown to customers at checkout.</summary>
    [HttpGet]
    public async Task<ActionResult<PaymentSettingsDto>> Get(CancellationToken ct)
    {
        var settings = await sender.Send(new GetPaymentSettingsQuery(), ct);
        return Ok(settings);
    }

    /// <summary>Updates the store's UPI ID, QR code image, and payment instructions.</summary>
    [HttpPut]
    [RequireSection("section-payment-settings-manage")]
    public async Task<ActionResult<PaymentSettingsDto>> Update(UpdatePaymentSettingsRequest request, CancellationToken ct)
    {
        var updatedByEmail = User.Identity?.Name;

        var result = await sender.Send(new UpdatePaymentSettingsCommand(
            request.UpiId,
            request.PayeeName,
            request.QrCodeImageUrl,
            request.Instructions,
            updatedByEmail), ct);

        return result.Succeeded ? Ok(result.Data) : BadRequest(new { errors = result.Errors });
    }
}
