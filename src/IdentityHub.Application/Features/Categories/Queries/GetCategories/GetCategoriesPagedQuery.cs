using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Categories.Queries.GetCategories;

public sealed record GetCategoriesPagedQuery(
    string? Search,
    bool? IsActive,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<CategoryDto>>;

public sealed class GetCategoriesPagedQueryHandler(ICategoryService categoryService)
    : IRequestHandler<GetCategoriesPagedQuery, PagedResult<CategoryDto>>
{
    public Task<PagedResult<CategoryDto>> Handle(GetCategoriesPagedQuery request, CancellationToken cancellationToken)
        => categoryService.GetPagedAsync(new CategoryListQuery(request.Search, request.IsActive, request.Page, request.PageSize), cancellationToken);
}
