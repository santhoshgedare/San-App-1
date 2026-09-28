using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Categories.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid Id) : IRequest<Result>;

public sealed class DeleteCategoryCommandHandler(ICategoryService categoryService)
    : IRequestHandler<DeleteCategoryCommand, Result>
{
    public Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        => categoryService.DeleteAsync(request.Id, cancellationToken);
}
