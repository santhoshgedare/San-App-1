using FluentValidation;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Items.Commands.CreateItem;

public sealed record CreateItemCommand(
    string Code,
    string Name,
    string? Description,
    string? Barcode,
    Guid CategoryId,
    string? UnitOfMeasurement,
    decimal Price,
    decimal CostPrice,
    int StockQuantity,
    bool IsActive,
    IReadOnlyList<ItemImageInput> Images,
    IReadOnlyList<ItemDocumentInput> Documents,
    IReadOnlyList<ItemVariantInput> Variants) : IRequest<Result<ItemDto>>;

public sealed class CreateItemCommandValidator : AbstractValidator<CreateItemCommand>
{
    public CreateItemCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateItemCommandHandler(IItemService itemService)
    : IRequestHandler<CreateItemCommand, Result<ItemDto>>
{
    public Task<Result<ItemDto>> Handle(CreateItemCommand request, CancellationToken cancellationToken)
        => itemService.CreateAsync(
            request.Code,
            request.Name,
            request.Description,
            request.Barcode,
            request.CategoryId,
            request.UnitOfMeasurement,
            request.Price,
            request.CostPrice,
            request.StockQuantity,
            request.IsActive,
            request.Images,
            request.Documents,
            request.Variants,
            cancellationToken);
}
