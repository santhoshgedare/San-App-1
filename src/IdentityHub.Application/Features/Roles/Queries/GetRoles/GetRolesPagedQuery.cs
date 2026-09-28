using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Roles.Queries.GetRoles;

/// <summary>Paged, searchable roles listing for scroll-pagination list pages.</summary>
public sealed record GetRolesPagedQuery(string? Search, int Page, int PageSize) : IRequest<PagedResult<RoleDto>>;

public sealed class GetRolesPagedQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetRolesPagedQuery, PagedResult<RoleDto>>
{
    public Task<PagedResult<RoleDto>> Handle(GetRolesPagedQuery request, CancellationToken cancellationToken)
        => identityService.GetRolesPagedAsync(
            new RoleListQuery { Search = request.Search, Page = request.Page, PageSize = request.PageSize },
            cancellationToken);
}
