using FluentValidation;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    string Name,
    string? Description,
    bool IsActive,
    string? UnitOfMeasurement,
    IReadOnlyList<CategoryVariantDefinitionInput> VariantDefinitions) : IRequest<Result<CategoryDto>>;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public sealed class CreateCategoryCommandHandler(ICategoryService categoryService)
    : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    public Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        => categoryService.CreateAsync(
            request.Name,
            request.Description,
            request.IsActive,
            request.UnitOfMeasurement,
            request.VariantDefinitions,
            cancellationToken);
}
