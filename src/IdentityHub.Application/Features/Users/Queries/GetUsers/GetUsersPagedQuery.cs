using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Users.Queries.GetUsers;

/// <summary>Paged, filterable, searchable users listing for scroll-pagination list pages.</summary>
public sealed record GetUsersPagedQuery(string? Search, string? Role, bool? IsActive, int Page, int PageSize)
    : IRequest<PagedResult<UserDto>>;

public sealed class GetUsersPagedQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetUsersPagedQuery, PagedResult<UserDto>>
{
    public Task<PagedResult<UserDto>> Handle(GetUsersPagedQuery request, CancellationToken cancellationToken)
        => identityService.GetUsersPagedAsync(
            new UserListQuery
            {
                Search = request.Search,
                Role = request.Role,
                IsActive = request.IsActive,
                Page = request.Page,
                PageSize = request.PageSize
            },
            cancellationToken);
}
