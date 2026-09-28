using IdentityHub.Application.Common.Models;

namespace IdentityHub.Application.Common.Interfaces;

public interface IPaymentSettingsService
{
    Task<PaymentSettingsDto> GetAsync(CancellationToken ct = default);
    Task<Result<PaymentSettingsDto>> UpdateAsync(
        string upiId,
        string? payeeName,
        string? qrCodeImageUrl,
        string? instructions,
        string? updatedByEmail,
        CancellationToken ct = default);
}
