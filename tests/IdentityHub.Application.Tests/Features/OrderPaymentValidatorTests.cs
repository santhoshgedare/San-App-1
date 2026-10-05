using FluentAssertions;
using IdentityHub.Application.Features.Orders.Commands.RecordOrderRefund;
using IdentityHub.Application.Features.Orders.Commands.UpdatePaymentStatus;
using Xunit;

namespace IdentityHub.Application.Tests.Features;

public sealed class RecordOrderRefundCommandValidatorTests
{
    private readonly RecordOrderRefundCommandValidator _validator = new();

    [Fact]
    public void Accepts_a_full_refund_request_with_reference()
    {
        var result = _validator.Validate(new RecordOrderRefundCommand(
            Guid.NewGuid(), 125.50m, "REFUND-UTR-123", "Customer cancellation"));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, "REFUND-UTR-123")]
    [InlineData(-1, "REFUND-UTR-123")]
    [InlineData(125.50, "")]
    public void Rejects_refunds_without_a_positive_amount_and_reference(decimal amount, string reference)
    {
        var result = _validator.Validate(new RecordOrderRefundCommand(
            Guid.NewGuid(), amount, reference, null));

        result.IsValid.Should().BeFalse();
    }
}

public sealed class UpdatePaymentStatusCommandValidatorTests
{
    private readonly UpdatePaymentStatusCommandValidator _validator = new();

    [Fact]
    public void Requires_the_refund_workflow_for_refunded_status()
    {
        var result = _validator.Validate(new UpdatePaymentStatusCommand(
            Guid.NewGuid(), "Refunded", "REFUND-UTR-123", null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(UpdatePaymentStatusCommand.PaymentStatus));
    }
}