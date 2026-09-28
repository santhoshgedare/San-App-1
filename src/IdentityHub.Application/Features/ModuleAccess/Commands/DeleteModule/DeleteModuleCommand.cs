using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.ModuleAccess.Commands.DeleteModule;

public sealed record DeleteModuleCommand(Guid ModuleId) : IRequest<Result>;

public sealed class DeleteModuleCommandHandler(IModuleAccessService moduleAccessService)
    : IRequestHandler<DeleteModuleCommand, Result>
{
    public Task<Result> Handle(DeleteModuleCommand request, CancellationToken cancellationToken)
        => moduleAccessService.DeleteModuleAsync(request.ModuleId, cancellationToken);
}
