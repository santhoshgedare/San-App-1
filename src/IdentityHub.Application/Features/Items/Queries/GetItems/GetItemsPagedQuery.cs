using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Items.Queries.GetItems;

public sealed record GetItemsPagedQuery(
    string? Search,
    Guid? CategoryId,
    bool? IsActive,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<ItemDto>>;

public sealed class GetItemsPagedQueryHandler(IItemService itemService)
    : IRequestHandler<GetItemsPagedQuery, PagedResult<ItemDto>>
{
    public Task<PagedResult<ItemDto>> Handle(GetItemsPagedQuery request, CancellationToken cancellationToken)
        => itemService.GetPagedAsync(new ItemListQuery(request.Search, request.CategoryId, request.IsActive, request.Page, request.PageSize), cancellationToken);
}
