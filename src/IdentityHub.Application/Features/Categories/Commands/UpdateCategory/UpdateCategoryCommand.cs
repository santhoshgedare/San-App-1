using FluentValidation;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    string? UnitOfMeasurement,
    IReadOnlyList<CategoryVariantDefinitionInput> VariantDefinitions) : IRequest<Result>;

public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public sealed class UpdateCategoryCommandHandler(ICategoryService categoryService)
    : IRequestHandler<UpdateCategoryCommand, Result>
{
    public Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        => categoryService.UpdateAsync(
            request.Id,
            request.Name,
            request.Description,
            request.IsActive,
            request.UnitOfMeasurement,
            request.VariantDefinitions,
            cancellationToken);
}
