using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.ModuleAccess.Commands.DeleteSection;

public sealed record DeleteSectionCommand(Guid SectionId) : IRequest<Result>;

public sealed class DeleteSectionCommandHandler(IModuleAccessService moduleAccessService)
    : IRequestHandler<DeleteSectionCommand, Result>
{
    public Task<Result> Handle(DeleteSectionCommand request, CancellationToken cancellationToken)
        => moduleAccessService.DeleteSectionAsync(request.SectionId, cancellationToken);
}
