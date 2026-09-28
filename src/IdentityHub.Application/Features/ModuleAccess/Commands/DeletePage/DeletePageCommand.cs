using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.ModuleAccess.Commands.DeletePage;

public sealed record DeletePageCommand(Guid PageId) : IRequest<Result>;

public sealed class DeletePageCommandHandler(IModuleAccessService moduleAccessService)
    : IRequestHandler<DeletePageCommand, Result>
{
    public Task<Result> Handle(DeletePageCommand request, CancellationToken cancellationToken)
        => moduleAccessService.DeletePageAsync(request.PageId, cancellationToken);
}
