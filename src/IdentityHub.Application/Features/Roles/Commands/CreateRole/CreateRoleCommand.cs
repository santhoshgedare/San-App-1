using FluentValidation;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Roles.Commands.CreateRole;

public sealed record CreateRoleCommand(string Name) : IRequest<Result>;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
    }
}

public sealed class CreateRoleCommandHandler(IIdentityService identityService) : IRequestHandler<CreateRoleCommand, Result>
{
    public Task<Result> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        => identityService.CreateRoleAsync(request.Name, cancellationToken);
}
