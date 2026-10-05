using FluentAssertions;
using IdentityHub.Application.Features.Auth.Commands.Login;
using IdentityHub.Application.Features.Auth.Commands.Register;
using IdentityHub.Application.Features.Users.Commands.UpdateUser;
using Xunit;

namespace IdentityHub.Application.Tests.Features;

public sealed class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Fails_when_email_is_invalid()
    {
        var result = _validator.Validate(new LoginCommand("not-an-email", "password123"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Email));
    }

    [Fact]
    public void Fails_when_password_is_empty()
    {
        var result = _validator.Validate(new LoginCommand("user@example.com", ""));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Password));
    }

    [Fact]
    public void Succeeds_for_valid_credentials()
    {
        var result = _validator.Validate(new LoginCommand("user@example.com", "password123"));

        result.IsValid.Should().BeTrue();
    }
}

public sealed class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Theory]
    [InlineData("", "password123", "Jane", "Doe")]
    [InlineData("user@example.com", "short", "Jane", "Doe")]
    [InlineData("user@example.com", "password123", "", "Doe")]
    [InlineData("user@example.com", "password123", "Jane", "")]
    public void Fails_for_invalid_input(string email, string password, string firstName, string lastName)
    {
        var result = _validator.Validate(new RegisterCommand(email, password, firstName, lastName, null));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Succeeds_for_valid_registration()
    {
        var result = _validator.Validate(new RegisterCommand("user@example.com", "password123", "Jane", "Doe", null));

        result.IsValid.Should().BeTrue();
    }
}

public sealed class UpdateUserCommandValidatorTests
{
    private readonly UpdateUserCommandValidator _validator = new();

    [Fact]
    public void Fails_when_user_id_is_empty()
    {
        var result = _validator.Validate(new UpdateUserCommand(Guid.Empty, "Jane", "Doe", true));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserCommand.UserId));
    }

    [Fact]
    public void Succeeds_for_valid_update()
    {
        var result = _validator.Validate(new UpdateUserCommand(Guid.NewGuid(), "Jane", "Doe", true));

        result.IsValid.Should().BeTrue();
    }
}

public sealed class CreateCategoryCommandValidatorTests
{
    private readonly IdentityHub.Application.Features.Categories.Commands.CreateCategory.CreateCategoryCommandValidator _validator = new();

    [Fact]
    public void Fails_when_name_is_empty()
    {
        var result = _validator.Validate(new IdentityHub.Application.Features.Categories.Commands.CreateCategory.CreateCategoryCommand(
            "",
            "Description",
            true,
            "Piece",
            []));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Succeeds_for_valid_create()
    {
        var result = _validator.Validate(new IdentityHub.Application.Features.Categories.Commands.CreateCategory.CreateCategoryCommand(
            "Electronics",
            "Electronic items",
            true,
            "Piece",
            []));

        result.IsValid.Should().BeTrue();
    }
}

public sealed class UpdateCategoryCommandValidatorTests
{
    private readonly IdentityHub.Application.Features.Categories.Commands.UpdateCategory.UpdateCategoryCommandValidator _validator = new();

    [Fact]
    public void Fails_when_id_is_empty()
    {
        var result = _validator.Validate(new IdentityHub.Application.Features.Categories.Commands.UpdateCategory.UpdateCategoryCommand(
            Guid.Empty,
            "Apparel",
            null,
            true,
            "Piece",
            []));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }

    [Fact]
    public void Succeeds_for_valid_update()
    {
        var result = _validator.Validate(new IdentityHub.Application.Features.Categories.Commands.UpdateCategory.UpdateCategoryCommand(
            Guid.NewGuid(),
            "Apparel",
            "Clothing & Fashion",
            true,
            "Piece",
            []));

        result.IsValid.Should().BeTrue();
    }
}

public sealed class CreateItemCommandValidatorTests
{
    private readonly IdentityHub.Application.Features.Items.Commands.CreateItem.CreateItemCommandValidator _validator = new();

    [Fact]
    public void Fails_when_code_is_empty()
    {
        var result = _validator.Validate(new IdentityHub.Application.Features.Items.Commands.CreateItem.CreateItemCommand(
            "",
            "T-Shirt",
            "Cotton T-Shirt",
            null,
            Guid.NewGuid(),
            "Piece",
            29.99m,
            15.00m,
            100,
            true,
            [],
            [],
            []));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Code");
    }

    [Fact]
    public void Succeeds_for_valid_item_command()
    {
        var result = _validator.Validate(new IdentityHub.Application.Features.Items.Commands.CreateItem.CreateItemCommand(
            "TSH-001",
            "T-Shirt",
            "Cotton T-Shirt",
            "1234567890",
            Guid.NewGuid(),
            "Piece",
            29.99m,
            15.00m,
            100,
            true,
            [],
            [],
            []));

        result.IsValid.Should().BeTrue();
    }
}

public sealed class UpdateItemCommandValidatorTests
{
    private readonly IdentityHub.Application.Features.Items.Commands.UpdateItem.UpdateItemCommandValidator _validator = new();

    [Fact]
    public void Fails_when_id_is_empty()
    {
        var result = _validator.Validate(new IdentityHub.Application.Features.Items.Commands.UpdateItem.UpdateItemCommand(
            Guid.Empty,
            "TSH-001",
            "T-Shirt",
            null,
            null,
            Guid.NewGuid(),
            "Piece",
            29.99m,
            15.00m,
            100,
            true,
            [],
            [],
            []));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }

    [Fact]
    public void Succeeds_for_valid_update_item()
    {
        var result = _validator.Validate(new IdentityHub.Application.Features.Items.Commands.UpdateItem.UpdateItemCommand(
            Guid.NewGuid(),
            "TSH-001",
            "T-Shirt",
            "Updated",
            "1234567890",
            Guid.NewGuid(),
            "Piece",
            34.99m,
            18.00m,
            120,
            true,
            [],
            [],
            []));

        result.IsValid.Should().BeTrue();
    }
}

public sealed class ItemPricingAndVariantValidationTests
{
    private readonly IdentityHub.Application.Features.Items.Commands.CreateItem.CreateItemCommandValidator _validator = new();

    [Fact]
    public void Fails_when_item_price_is_negative()
    {
        var result = _validator.Validate(new IdentityHub.Application.Features.Items.Commands.CreateItem.CreateItemCommand(
            "CODE-01",
            "Product",
            null,
            null,
            Guid.NewGuid(),
            "Piece",
            -10.0m,
            5.0m,
            10,
            true,
            [],
            [],
            []));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    [Fact]
    public void Fails_when_item_name_exceeds_max_length()
    {
        var longName = new string('A', 250);
        var result = _validator.Validate(new IdentityHub.Application.Features.Items.Commands.CreateItem.CreateItemCommand(
            "CODE-01",
            longName,
            null,
            null,
            Guid.NewGuid(),
            "Piece",
            10.0m,
            5.0m,
            10,
            true,
            [],
            [],
            []));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }
}

public sealed class CreateOrderCommandValidatorTests
{
    private readonly IdentityHub.Application.Features.Orders.Commands.CreateOrder.CreateOrderCommandValidator _validator = new();

    [Fact]
    public void Fails_when_items_are_empty()
    {
        var result = _validator.Validate(new IdentityHub.Application.Features.Orders.Commands.CreateOrder.CreateOrderCommand(
            Guid.NewGuid(),
            "John Doe",
            "john@example.com",
            "+1234567890",
            "123 Main St",
            null,
            null,
            "UpiQr",
            "UPI-TXN-12345",
            null,
            []));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Items");
    }

    [Fact]
    public void Fails_when_customer_email_is_invalid()
    {
        var result = _validator.Validate(new IdentityHub.Application.Features.Orders.Commands.CreateOrder.CreateOrderCommand(
            Guid.NewGuid(),
            "John Doe",
            "not-an-email",
            "+1234567890",
            "123 Main St",
            null,
            null,
            "UpiQr",
            "UPI-TXN-12345",
            null,
            [
                new IdentityHub.Application.Common.Models.OrderItemInput(
                    Guid.NewGuid(),
                    null,
                    "CODE-1",
                    "Item 1",
                    null,
                    null,
                    null,
                    null,
                    50.0m,
                    1)
            ]));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerEmail");
    }

    [Fact]
    public void Succeeds_without_payment_reference_number_until_payment_is_verified_later()
    {
        var result = _validator.Validate(new IdentityHub.Application.Features.Orders.Commands.CreateOrder.CreateOrderCommand(
            Guid.NewGuid(),
            "John Doe",
            "john@example.com",
            "+1234567890",
            "123 Main St",
            null,
            null,
            "UpiQr",
            null,
            null,
            [
                new IdentityHub.Application.Common.Models.OrderItemInput(
                    Guid.NewGuid(),
                    null,
                    "CODE-1",
                    "Item 1",
                    null,
                    null,
                    null,
                    null,
                    50.0m,
                    1)
            ]));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Succeeds_for_valid_order_request()
    {
        var result = _validator.Validate(new IdentityHub.Application.Features.Orders.Commands.CreateOrder.CreateOrderCommand(
            Guid.NewGuid(),
            "John Doe",
            "john@example.com",
            "+1234567890",
            "123 Main St",
            null,
            null,
            "UpiQr",
            "UPI-TXN-12345",
            null,
            [
                new IdentityHub.Application.Common.Models.OrderItemInput(
                    Guid.NewGuid(),
                    null,
                    "CODE-1",
                    "Item 1",
                    null,
                    null,
                    null,
                    null,
                    50.0m,
                    2)
            ]));

        result.IsValid.Should().BeTrue();
    }
}




