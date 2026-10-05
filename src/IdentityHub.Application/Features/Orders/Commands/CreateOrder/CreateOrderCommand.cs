using FluentValidation;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Domain.Enums;
using MediatR;

namespace IdentityHub.Application.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    Guid? CustomerId,
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    string ShippingAddress,
    string? BillingAddress,
    string? OrderNotes,
    string PaymentMethod,
    string? PaymentReferenceNumber,
    string? OfflinePaymentNotes,
    IReadOnlyList<OrderItemInput> Items) : IRequest<Result<OrderDto>>;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.CustomerPhone).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ShippingAddress).NotEmpty().MaximumLength(500);
        RuleFor(x => x.PaymentReferenceNumber)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.PaymentReferenceNumber))
            .WithMessage("UPI transaction reference cannot exceed 100 characters.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required in the order.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ItemId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}

public sealed class CreateOrderCommandHandler(IOrderService orderService)
    : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var paymentMethod = Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var pm)
            ? pm
            : PaymentMethod.UpiQr;

        return await orderService.CreateAsync(
            request.CustomerId,
            request.CustomerName,
            request.CustomerEmail,
            request.CustomerPhone,
            request.ShippingAddress,
            request.BillingAddress,
            request.OrderNotes,
            paymentMethod,
            request.PaymentReferenceNumber,
            request.OfflinePaymentNotes,
            request.Items,
            ct);
    }
}
