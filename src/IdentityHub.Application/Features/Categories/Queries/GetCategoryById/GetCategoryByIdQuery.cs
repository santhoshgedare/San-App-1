using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Categories.Queries.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto?>;

public sealed class GetCategoryByIdQueryHandler(ICategoryService categoryService)
    : IRequestHandler<GetCategoryByIdQuery, CategoryDto?>
{
    public Task<CategoryDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        => categoryService.GetByIdAsync(request.Id, cancellationToken);
}
