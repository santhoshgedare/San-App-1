using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.ModuleAccess.Queries.GetRoleAccess;

/// <summary>Lists every role together with the section keys it currently has access to.</summary>
public sealed record GetAllRoleAccessQuery : IRequest<IReadOnlyList<RoleAccessDto>>;

public sealed class GetAllRoleAccessQueryHandler(IModuleAccessService moduleAccessService)
    : IRequestHandler<GetAllRoleAccessQuery, IReadOnlyList<RoleAccessDto>>
{
    public Task<IReadOnlyList<RoleAccessDto>> Handle(GetAllRoleAccessQuery request, CancellationToken cancellationToken)
        => moduleAccessService.GetAllRoleAccessAsync(cancellationToken);
}

/// <summary>Gets the section access granted to a single role.</summary>
public sealed record GetRoleAccessByIdQuery(Guid RoleId) : IRequest<RoleAccessDto?>;

public sealed class GetRoleAccessByIdQueryHandler(IModuleAccessService moduleAccessService)
    : IRequestHandler<GetRoleAccessByIdQuery, RoleAccessDto?>
{
    public Task<RoleAccessDto?> Handle(GetRoleAccessByIdQuery request, CancellationToken cancellationToken)
        => moduleAccessService.GetRoleAccessAsync(request.RoleId, cancellationToken);
}
