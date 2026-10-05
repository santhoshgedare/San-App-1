using FluentValidation;
using IdentityHub.Application.Common.Models;

namespace IdentityHub.Application.Common.Validation;

public sealed class AddressInputValidator : AbstractValidator<AddressInput>
{
    public AddressInputValidator()
    {
        RuleFor(x => x.Line1).NotEmpty().WithMessage("Address line is required.").MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().WithMessage("City is required.").MaximumLength(100);
        RuleFor(x => x.State).NotEmpty().WithMessage("State is required.").MaximumLength(100);
        RuleFor(x => x.PostalCode).NotEmpty().WithMessage("Postal code is required.").MaximumLength(20);
        RuleFor(x => x.Country).NotEmpty().WithMessage("Country is required.").MaximumLength(100);
    }
}