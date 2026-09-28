using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Roles.Commands.DeleteRole;

public sealed record DeleteRoleCommand(Guid RoleId) : IRequest<Result>;

public sealed class DeleteRoleCommandHandler(IIdentityService identityService) : IRequestHandler<DeleteRoleCommand, Result>
{
    public Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        => identityService.DeleteRoleAsync(request.RoleId, cancellationToken);
}
