using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.ModuleAccess.Queries.GetModuleTree;

/// <summary>Returns the full Module &gt; Page &gt; Section hierarchy for master management screens.</summary>
public sealed record GetModuleTreeQuery : IRequest<IReadOnlyList<ModuleDto>>;

public sealed class GetModuleTreeQueryHandler(IModuleAccessService moduleAccessService)
    : IRequestHandler<GetModuleTreeQuery, IReadOnlyList<ModuleDto>>
{
    public Task<IReadOnlyList<ModuleDto>> Handle(GetModuleTreeQuery request, CancellationToken cancellationToken)
        => moduleAccessService.GetModuleTreeAsync(cancellationToken);
}
