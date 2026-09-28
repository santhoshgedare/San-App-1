using FluentValidation;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Users.Commands.AssignRoles;

public sealed record AssignRolesCommand(Guid UserId, IReadOnlyCollection<string> Roles) : IRequest<Result>;

public sealed class AssignRolesCommandValidator : AbstractValidator<AssignRolesCommand>
{
    public AssignRolesCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Roles).NotNull();
    }
}

public sealed class AssignRolesCommandHandler(IIdentityService identityService) : IRequestHandler<AssignRolesCommand, Result>
{
    public Task<Result> Handle(AssignRolesCommand request, CancellationToken cancellationToken)
        => identityService.AssignRolesAsync(request.UserId, request.Roles, cancellationToken);
}
