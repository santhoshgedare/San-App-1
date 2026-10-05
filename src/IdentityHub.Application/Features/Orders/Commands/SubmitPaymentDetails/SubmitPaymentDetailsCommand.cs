using FluentValidation;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Orders.Commands.SubmitPaymentDetails;

public sealed record SubmitPaymentDetailsCommand(
    Guid Id,
    string PaymentReferenceNumber,
    string? OfflinePaymentNotes) : IRequest<Result>;

public sealed class SubmitPaymentDetailsCommandValidator : AbstractValidator<SubmitPaymentDetailsCommand>
{
    public SubmitPaymentDetailsCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.PaymentReferenceNumber)
            .Must(reference => !string.IsNullOrWhiteSpace(reference))
            .MaximumLength(100);
        RuleFor(x => x.OfflinePaymentNotes).MaximumLength(500);
    }
}

public sealed class SubmitPaymentDetailsCommandHandler(IOrderService orderService)
    : IRequestHandler<SubmitPaymentDetailsCommand, Result>
{
    public Task<Result> Handle(SubmitPaymentDetailsCommand request, CancellationToken ct) =>
        orderService.SubmitPaymentDetailsAsync(
            request.Id,
            request.PaymentReferenceNumber,
            request.OfflinePaymentNotes,
            ct);
}