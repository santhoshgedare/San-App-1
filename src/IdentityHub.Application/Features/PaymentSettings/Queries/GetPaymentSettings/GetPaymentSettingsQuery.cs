using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.PaymentSettings.Queries.GetPaymentSettings;

public sealed record GetPaymentSettingsQuery : IRequest<PaymentSettingsDto>;

public sealed class GetPaymentSettingsQueryHandler(IPaymentSettingsService paymentSettingsService)
    : IRequestHandler<GetPaymentSettingsQuery, PaymentSettingsDto>
{
    public Task<PaymentSettingsDto> Handle(GetPaymentSettingsQuery request, CancellationToken ct) =>
        paymentSettingsService.GetAsync(ct);
}
