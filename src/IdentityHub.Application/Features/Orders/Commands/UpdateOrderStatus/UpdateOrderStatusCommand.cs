using FluentValidation;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Domain.Enums;
using MediatR;

namespace IdentityHub.Application.Features.Orders.Commands.UpdateOrderStatus;

public sealed record UpdateOrderStatusCommand(
    Guid Id,
    string Status,
    string? TrackingNumber,
    string? ShippingCarrier) : IRequest<Result>;

public sealed class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Status).NotEmpty().IsEnumName(typeof(OrderStatus), caseSensitive: false);
    }
}

public sealed class UpdateOrderStatusCommandHandler(IOrderService orderService)
    : IRequestHandler<UpdateOrderStatusCommand, Result>
{
    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken ct)
    {
        var status = Enum.Parse<OrderStatus>(request.Status, true);
        return await orderService.UpdateStatusAsync(request.Id, status, request.TrackingNumber, request.ShippingCarrier, ct);
    }
}
