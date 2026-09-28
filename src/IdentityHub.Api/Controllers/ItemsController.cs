using IdentityHub.Api.Contracts;
using IdentityHub.Application.Common.Models;
using IdentityHub.Application.Features.Items.Commands.CreateItem;
using IdentityHub.Application.Features.Items.Commands.DeleteItem;
using IdentityHub.Application.Features.Items.Commands.UpdateItem;
using IdentityHub.Application.Features.Items.Queries.GetItemById;
using IdentityHub.Application.Features.Items.Queries.GetItems;
using IdentityHub.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ItemsController(ISender sender) : ControllerBase
{
    /// <summary>Lists all active items. Public/anonymous access for catalog browsing.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ItemDto>>> GetAll(CancellationToken ct)
        => Ok(await sender.Send(new GetItemsQuery(), ct));

    /// <summary>Lists items with search, category filter, active filter, and pagination. Public/anonymous access for catalog browsing.</summary>
    [HttpGet("paged")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<ItemDto>>> GetPaged(
        [FromQuery] string? search,
        [FromQuery] Guid? categoryId,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetItemsPagedQuery(search, categoryId, isActive, page, pageSize), ct));

    /// <summary>Gets a single item by id including its images, documents, and variants. Public/anonymous access for catalog browsing.</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ItemDto>> GetById(Guid id, CancellationToken ct)
    {
        var item = await sender.Send(new GetItemByIdQuery(id), ct);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>Creates a new item master with 1:N images, documents, and variant-based pricing.</summary>
    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<ActionResult<ItemDto>> Create(CreateItemRequest request, CancellationToken ct)
    {
        var imageInputs = (request.Images ?? [])
            .Select(img => new ItemImageInput(img.Id, img.Url, img.FileName, img.Caption, img.IsPrimary, img.SortOrder))
            .ToList();

        var documentInputs = (request.Documents ?? [])
            .Select(doc => new ItemDocumentInput(doc.Id, doc.Url, doc.FileName, doc.DocumentType, doc.FileSizeBytes, doc.Description))
            .ToList();

        var variantInputs = (request.Variants ?? [])
            .Select(v => new ItemVariantInput(v.Id, v.Sku, v.Name, v.Barcode, v.AttributesJson, v.Price, v.CostPrice, v.StockQuantity, v.IsActive))
            .ToList();

        var result = await sender.Send(new CreateItemCommand(
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
            imageInputs,
            documentInputs,
            variantInputs), ct);

        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data)
            : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Updates an existing item master.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<IActionResult> Update(Guid id, UpdateItemRequest request, CancellationToken ct)
    {
        var imageInputs = (request.Images ?? [])
            .Select(img => new ItemImageInput(img.Id, img.Url, img.FileName, img.Caption, img.IsPrimary, img.SortOrder))
            .ToList();

        var documentInputs = (request.Documents ?? [])
            .Select(doc => new ItemDocumentInput(doc.Id, doc.Url, doc.FileName, doc.DocumentType, doc.FileSizeBytes, doc.Description))
            .ToList();

        var variantInputs = (request.Variants ?? [])
            .Select(v => new ItemVariantInput(v.Id, v.Sku, v.Name, v.Barcode, v.AttributesJson, v.Price, v.CostPrice, v.StockQuantity, v.IsActive))
            .ToList();

        var result = await sender.Send(new UpdateItemCommand(
            id,
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
            imageInputs,
            documentInputs,
            variantInputs), ct);

        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Soft-deletes an item.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteItemCommand(id), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }
}
