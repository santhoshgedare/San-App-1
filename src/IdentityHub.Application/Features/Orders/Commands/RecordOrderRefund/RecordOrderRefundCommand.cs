using FluentValidation;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Orders.Commands.RecordOrderRefund;

public sealed record RecordOrderRefundCommand(
    Guid Id,
    decimal RefundAmount,
    string RefundReferenceNumber,
    string? RefundNotes) : IRequest<Result>;

public sealed class RecordOrderRefundCommandValidator : AbstractValidator<RecordOrderRefundCommand>
{
    public RecordOrderRefundCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.RefundAmount).GreaterThan(0);
        RuleFor(x => x.RefundReferenceNumber)
            .Must(reference => !string.IsNullOrWhiteSpace(reference))
            .MaximumLength(100);
        RuleFor(x => x.RefundNotes).MaximumLength(1000);
    }
}

public sealed class RecordOrderRefundCommandHandler(IOrderService orderService)
    : IRequestHandler<RecordOrderRefundCommand, Result>
{
    public Task<Result> Handle(RecordOrderRefundCommand request, CancellationToken ct) =>
        orderService.RecordRefundAsync(
            request.Id,
            request.RefundAmount,
            request.RefundReferenceNumber,
            request.RefundNotes,
            ct);
}