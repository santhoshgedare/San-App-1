using FluentValidation;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.PaymentSettings.Commands.UpdatePaymentSettings;

public sealed record UpdatePaymentSettingsCommand(
    string UpiId,
    string? PayeeName,
    string? QrCodeImageUrl,
    string? Instructions,
    string? UpdatedByEmail) : IRequest<Result<PaymentSettingsDto>>;

public sealed class UpdatePaymentSettingsCommandValidator : AbstractValidator<UpdatePaymentSettingsCommand>
{
    public UpdatePaymentSettingsCommandValidator()
    {
        RuleFor(x => x.UpiId).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PayeeName).MaximumLength(200);
        RuleFor(x => x.Instructions).MaximumLength(1000);
    }
}

public sealed class UpdatePaymentSettingsCommandHandler(IPaymentSettingsService paymentSettingsService)
    : IRequestHandler<UpdatePaymentSettingsCommand, Result<PaymentSettingsDto>>
{
    public Task<Result<PaymentSettingsDto>> Handle(UpdatePaymentSettingsCommand request, CancellationToken ct) =>
        paymentSettingsService.UpdateAsync(
            request.UpiId,
            request.PayeeName,
            request.QrCodeImageUrl,
            request.Instructions,
            request.UpdatedByEmail,
            ct);
}
