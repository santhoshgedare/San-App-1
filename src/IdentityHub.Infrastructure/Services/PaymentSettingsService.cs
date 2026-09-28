using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Domain.Constants;
using IdentityHub.Domain.Entities;
using IdentityHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityHub.Infrastructure.Services;

public sealed class PaymentSettingsService(
    AppDbContext db,
    IActivityLogService activityLogs) : IPaymentSettingsService
{
    public async Task<PaymentSettingsDto> GetAsync(CancellationToken ct = default)
    {
        var settings = await db.PaymentSettings.FirstOrDefaultAsync(ct);
        settings ??= await CreateDefaultAsync(ct);

        return MapToDto(settings);
    }

    public async Task<Result<PaymentSettingsDto>> UpdateAsync(
        string upiId,
        string? payeeName,
        string? qrCodeImageUrl,
        string? instructions,
        string? updatedByEmail,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(upiId))
        {
            return Result<PaymentSettingsDto>.Failure("UPI ID is required.");
        }

        var settings = await db.PaymentSettings.FirstOrDefaultAsync(ct);
        settings ??= await CreateDefaultAsync(ct);

        settings.UpiId = upiId.Trim();
        settings.PayeeName = payeeName?.Trim();
        settings.QrCodeImageUrl = qrCodeImageUrl;
        settings.Instructions = instructions?.Trim();
        settings.UpdatedAt = DateTimeOffset.UtcNow;
        settings.UpdatedByEmail = updatedByEmail;

        await db.SaveChangesAsync(ct);

        await activityLogs.LogAsync(
            EntityTypes.PaymentSettings,
            settings.Id.ToString(),
            "Updated",
            $"UPI payment settings updated (UPI ID: {settings.UpiId}) by {updatedByEmail ?? "system"}",
            ct);

        return Result<PaymentSettingsDto>.Success(MapToDto(settings));
    }

    private async Task<PaymentSettings> CreateDefaultAsync(CancellationToken ct)
    {
        var settings = new PaymentSettings
        {
            UpiId = string.Empty,
            PayeeName = null,
            QrCodeImageUrl = null,
            Instructions = "Scan the QR code using any UPI app and enter the transaction reference number after payment."
        };

        db.PaymentSettings.Add(settings);
        await db.SaveChangesAsync(ct);
        return settings;
    }

    private static PaymentSettingsDto MapToDto(PaymentSettings s) => new()
    {
        Id = s.Id,
        UpiId = s.UpiId,
        PayeeName = s.PayeeName,
        QrCodeImageUrl = s.QrCodeImageUrl,
        Instructions = s.Instructions,
        UpdatedAt = s.UpdatedAt
    };
}
