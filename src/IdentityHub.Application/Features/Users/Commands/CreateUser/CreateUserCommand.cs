using FluentValidation;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Users.Commands.CreateUser;

/// <summary>Admin-initiated user creation (distinct from self-service Register — no tokens issued).</summary>
public sealed record CreateUserCommand(string Email, string Password, string FirstName, string LastName, string PhoneNumber, AddressInput Address, IReadOnlyCollection<string> Roles)
    : IRequest<Result<UserDto>>;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone number is required.").MaximumLength(30);
        RuleFor(x => x.Address).NotNull().WithMessage("An address is required.").SetValidator(new IdentityHub.Application.Common.Validation.AddressInputValidator());
    }
}

public sealed class CreateUserCommandHandler(IIdentityService identityService, IAddressService addressService) : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var result = await identityService.RegisterAsync(request.Email, request.Password, request.FirstName, request.LastName, request.PhoneNumber, cancellationToken);
        if (!result.Succeeded)
        {
            return result;
        }

        await addressService.CreateAsync(result.Data!.Id, request.Address with { IsDefault = true }, cancellationToken);

        if (request.Roles.Count > 0)
        {
            var rolesResult = await identityService.AssignRolesAsync(result.Data!.Id, request.Roles, cancellationToken);
            if (!rolesResult.Succeeded)
            {
                return Result<UserDto>.Failure(rolesResult.Errors);
            }
        }

        var user = await identityService.FindByIdAsync(result.Data!.Id, cancellationToken);
        return Result<UserDto>.Success(user!);
    }
}
