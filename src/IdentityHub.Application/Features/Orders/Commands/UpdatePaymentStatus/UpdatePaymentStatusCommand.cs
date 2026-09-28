using FluentValidation;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Domain.Enums;
using MediatR;

namespace IdentityHub.Application.Features.Orders.Commands.UpdatePaymentStatus;

public sealed record UpdatePaymentStatusCommand(
    Guid Id,
    string PaymentStatus,
    string? PaymentReferenceNumber,
    string? OfflinePaymentNotes) : IRequest<Result>;

public sealed class UpdatePaymentStatusCommandValidator : AbstractValidator<UpdatePaymentStatusCommand>
{
    public UpdatePaymentStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.PaymentStatus).NotEmpty().IsEnumName(typeof(PaymentStatus), caseSensitive: false);
    }
}

public sealed class UpdatePaymentStatusCommandHandler(IOrderService orderService)
    : IRequestHandler<UpdatePaymentStatusCommand, Result>
{
    public async Task<Result> Handle(UpdatePaymentStatusCommand request, CancellationToken ct)
    {
        var paymentStatus = Enum.Parse<PaymentStatus>(request.PaymentStatus, true);
        return await orderService.UpdatePaymentStatusAsync(
            request.Id,
            paymentStatus,
            request.PaymentReferenceNumber,
            request.OfflinePaymentNotes,
            ct);
    }
}
