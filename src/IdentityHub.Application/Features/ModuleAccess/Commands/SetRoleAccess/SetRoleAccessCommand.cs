using FluentValidation;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.ModuleAccess.Commands.SetRoleAccess;

/// <summary>Replaces the full set of section keys a role is granted access to.</summary>
public sealed record SetRoleAccessCommand(Guid RoleId, IReadOnlyCollection<string> SectionKeys) : IRequest<Result>;

public sealed class SetRoleAccessCommandValidator : AbstractValidator<SetRoleAccessCommand>
{
    public SetRoleAccessCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
    }
}

public sealed class SetRoleAccessCommandHandler(IModuleAccessService moduleAccessService)
    : IRequestHandler<SetRoleAccessCommand, Result>
{
    public Task<Result> Handle(SetRoleAccessCommand request, CancellationToken cancellationToken)
        => moduleAccessService.SetRoleAccessAsync(request.RoleId, request.SectionKeys, cancellationToken);
}
