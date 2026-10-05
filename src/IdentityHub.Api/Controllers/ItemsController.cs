using System.Security.Claims;
using IdentityHub.Api.Contracts;
using IdentityHub.Api.Authorization;
using IdentityHub.Application.Common.Interfaces;
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
public sealed class ItemsController(ISender sender, IItemService itemService, ISellerService sellers) : ControllerBase
{
    /// <summary>Lists all active items. Public/anonymous access for catalog browsing.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ItemDto>>> GetAll(CancellationToken ct)
    {
        var items = await sender.Send(new GetItemsQuery(), ct);
        return Ok(items.Select(HideInternalCosts).ToArray());
    }

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
    {
        var result = await sender.Send(new GetItemsPagedQuery(search, categoryId, isActive, page, pageSize), ct);
        return Ok(new PagedResult<ItemDto>
        {
            Items = result.Items.Select(HideInternalCosts).ToArray(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        });
    }

    /// <summary>Gets a single item by id including its images, documents, and variants. Public/anonymous access for catalog browsing.</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ItemDto>> GetById(Guid id, CancellationToken ct)
    {
        var item = await sender.Send(new GetItemByIdQuery(id), ct);
        return item is null ? NotFound() : Ok(HideInternalCosts(item));
    }

    /// <summary>Gets up to 5 active items from the same category, most purchased first. Public/anonymous access.</summary>
    [HttpGet("{id:guid}/similar")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ItemDto>>> GetSimilar(Guid id, [FromQuery] int take = 5, CancellationToken ct = default)
    {
        var items = await itemService.GetSimilarAsync(id, take, ct);
        return Ok(items.Select(HideInternalCosts).ToArray());
    }

    /// <summary>Gets full item details for users granted inventory management access.</summary>
    [HttpGet("management/{id:guid}")]
    [RequireSection("section-items-manage")]
    public async Task<ActionResult<ItemDto>> GetManagementById(Guid id, CancellationToken ct)
    {
        var scope = await ResolveScopeAsync(ct);
        var item = await sender.Send(new GetItemByIdQuery(id), ct);
        if (item is null || (scope.Scoped && item.SellerId != scope.SellerId)) return NotFound();
        return Ok(item);
    }

    /// <summary>Lists items including internal unit costs for inventory management.</summary>
    [HttpGet("management/paged")]
    [RequireSection("section-items-manage")]
    public async Task<ActionResult<PagedResult<ItemDto>>> GetManagementPaged(
        [FromQuery] string? search,
        [FromQuery] Guid? categoryId,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var scope = await ResolveScopeAsync(ct);
        if (!scope.Scoped)
        {
            return Ok(await sender.Send(new GetItemsPagedQuery(search, categoryId, isActive, page, pageSize), ct));
        }

        if (scope.SellerId is null) return Ok(new PagedResult<ItemDto> { Page = page, PageSize = pageSize });
        return Ok(await itemService.GetPagedAsync(new ItemListQuery(search, categoryId, isActive, page, pageSize, scope.SellerId), ct));
    }

    /// <summary>Creates a new item master with 1:N images, documents, and variant-based pricing.</summary>
    [HttpPost]
    [RequireSection("section-items-manage")]
    public async Task<ActionResult<ItemDto>> Create(CreateItemRequest request, [FromQuery] Guid? sellerId, CancellationToken ct)
    {
        var scope = await ResolveScopeAsync(ct);
        if (scope.Scoped && scope.SellerId is null) return Forbid();

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

        if (result.Succeeded)
        {
            var owner = scope.Scoped ? scope.SellerId : sellerId;
            if (owner.HasValue)
            {
                await itemService.AssignSellerAsync(result.Data!.Id, owner, ct);
            }
        }

        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data)
            : BadRequest(new { errors = result.Errors });
    }

    /// <summary>Updates an existing item master.</summary>
    [HttpPut("{id:guid}")]
    [RequireSection("section-items-manage")]
    public async Task<IActionResult> Update(Guid id, UpdateItemRequest request, CancellationToken ct)
    {
        if (!await CanManageAsync(id, ct)) return NotFound();

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
    [RequireSection("section-items-manage")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        if (!await CanManageAsync(id, ct)) return NotFound();

        var result = await sender.Send(new DeleteItemCommand(id), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }

    private async Task<(bool Scoped, Guid? SellerId)> ResolveScopeAsync(CancellationToken ct)
    {
        if (User.IsInRole(Roles.Admin) || !User.IsInRole(Roles.Manager)) return (false, null);
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(raw, out var userId)) return (true, null);
        return (true, await sellers.GetSellerIdForUserAsync(userId, ct));
    }

    private async Task<bool> CanManageAsync(Guid itemId, CancellationToken ct)
    {
        var scope = await ResolveScopeAsync(ct);
        if (!scope.Scoped) return true;
        return scope.SellerId is not null && await itemService.GetSellerIdAsync(itemId, ct) == scope.SellerId;
    }

    private static ItemDto HideInternalCosts(ItemDto item) => new()
    {
        Id = item.Id,
        SellerId = item.SellerId,
        SellerName = item.SellerName,
        Code = item.Code,
        Name = item.Name,
        Description = item.Description,
        Barcode = item.Barcode,
        CategoryId = item.CategoryId,
        CategoryName = item.CategoryName,
        UnitOfMeasurement = item.UnitOfMeasurement,
        Price = item.Price,
        CostPrice = null,
        StockQuantity = item.StockQuantity,
        IsActive = item.IsActive,
        CreatedAt = item.CreatedAt,
        Images = item.Images,
        Documents = item.Documents,
        Variants = item.Variants.Select(variant => new ItemVariantDto
        {
            Id = variant.Id,
            ItemId = variant.ItemId,
            Sku = variant.Sku,
            Name = variant.Name,
            Barcode = variant.Barcode,
            AttributesJson = variant.AttributesJson,
            Price = variant.Price,
            CostPrice = null,
            StockQuantity = variant.StockQuantity,
            IsActive = variant.IsActive
        }).ToArray()
    };
}
