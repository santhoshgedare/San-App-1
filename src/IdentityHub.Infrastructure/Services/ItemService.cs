using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Domain.Constants;
using IdentityHub.Domain.Entities;
using IdentityHub.Domain.Enums;
using IdentityHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityHub.Infrastructure.Services;

public sealed class ItemService(AppDbContext db, IActivityLogService activityLog) : IItemService
{
    public async Task<IReadOnlyList<ItemDto>> GetAllAsync(CancellationToken ct)
    {
        var items = await db.Items
            .Include(i => i.Category)
            .Include(i => i.Images)
            .Include(i => i.Documents)
            .Include(i => i.Variants)
            .OrderBy(i => i.Name)
            .ToListAsync(ct);

        return items.Select(ToDto).ToList();
    }

    public async Task<ItemDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var item = await db.Items
            .Include(i => i.Category)
            .Include(i => i.Images)
            .Include(i => i.Documents)
            .Include(i => i.Variants)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

        return item is null ? null : ToDto(item);
    }

    public async Task<PagedResult<ItemDto>> GetPagedAsync(ItemListQuery query, CancellationToken ct)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var itemsQuery = db.Items
            .Include(i => i.Category)
            .Include(i => i.Images)
            .Include(i => i.Documents)
            .Include(i => i.Variants)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            itemsQuery = itemsQuery.Where(i =>
                i.Code.Contains(term) ||
                i.Name.Contains(term) ||
                (i.Description != null && i.Description.Contains(term)) ||
                (i.Barcode != null && i.Barcode.Contains(term)) ||
                i.Category!.Name.Contains(term));
        }

        if (query.CategoryId.HasValue)
        {
            itemsQuery = itemsQuery.Where(i => i.CategoryId == query.CategoryId.Value);
        }

        if (query.IsActive.HasValue)
        {
            itemsQuery = itemsQuery.Where(i => i.IsActive == query.IsActive.Value);
        }

        var totalCount = await itemsQuery.CountAsync(ct);
        var pageOfItems = await itemsQuery
            .OrderBy(i => i.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<ItemDto>
        {
            Items = pageOfItems.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Result<ItemDto>> CreateAsync(
        string code,
        string name,
        string? description,
        string? barcode,
        Guid categoryId,
        string? unitOfMeasurement,
        decimal price,
        decimal costPrice,
        int stockQuantity,
        bool isActive,
        IReadOnlyList<ItemImageInput> images,
        IReadOnlyList<ItemDocumentInput> documents,
        IReadOnlyList<ItemVariantInput> variants,
        CancellationToken ct)
    {
        var trimmedCode = code.Trim();
        var trimmedName = name.Trim();

        if (string.IsNullOrWhiteSpace(trimmedCode))
        {
            return Result<ItemDto>.Failure("Item code is required.");
        }

        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            return Result<ItemDto>.Failure("Item name is required.");
        }

        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == categoryId, ct);
        if (category is null)
        {
            return Result<ItemDto>.Failure("Selected category does not exist.");
        }

        var codeExists = await db.Items.AnyAsync(i => i.Code == trimmedCode, ct);
        if (codeExists)
        {
            return Result<ItemDto>.Failure($"Item with code '{trimmedCode}' already exists.");
        }

        var uom = ParseUnitOfMeasurement(unitOfMeasurement, category.UnitOfMeasurement);

        var item = new Item
        {
            Code = trimmedCode,
            Name = trimmedName,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Barcode = string.IsNullOrWhiteSpace(barcode) ? null : barcode.Trim(),
            CategoryId = categoryId,
            UnitOfMeasurement = uom,
            Price = price,
            CostPrice = costPrice,
            StockQuantity = stockQuantity,
            IsActive = isActive,
            CreatedAt = DateTimeOffset.UtcNow,
            Images = images.Select((img, index) => new ItemImage
            {
                Url = img.Url,
                FileName = img.FileName,
                Caption = img.Caption,
                IsPrimary = img.IsPrimary,
                SortOrder = img.SortOrder > 0 ? img.SortOrder : index + 1,
                UploadedAt = DateTimeOffset.UtcNow
            }).ToList(),
            Documents = documents.Select(doc => new ItemDocument
            {
                Url = doc.Url,
                FileName = doc.FileName,
                DocumentType = doc.DocumentType,
                FileSizeBytes = doc.FileSizeBytes,
                Description = doc.Description,
                UploadedAt = DateTimeOffset.UtcNow
            }).ToList(),
            Variants = variants.Select(v => new ItemVariant
            {
                Sku = string.IsNullOrWhiteSpace(v.Sku) ? $"{trimmedCode}-{Guid.NewGuid().ToString()[..6].ToUpper()}" : v.Sku.Trim(),
                Name = v.Name?.Trim() ?? string.Empty,
                Barcode = string.IsNullOrWhiteSpace(v.Barcode) ? null : v.Barcode.Trim(),
                AttributesJson = string.IsNullOrWhiteSpace(v.AttributesJson) ? "{}" : v.AttributesJson,
                Price = v.Price,
                CostPrice = v.CostPrice,
                StockQuantity = v.StockQuantity,
                IsActive = v.IsActive
            }).ToList()
        };

        // Ensure at least one primary image if images exist
        if (item.Images.Count > 0 && !item.Images.Any(i => i.IsPrimary))
        {
            item.Images[0].IsPrimary = true;
        }

        db.Items.Add(item);
        await db.SaveChangesAsync(ct);

        await activityLog.LogAsync(
            EntityTypes.Item,
            item.Id.ToString(),
            "Created",
            $"Item {item.Code} ({item.Name}) created with {item.Variants.Count} variant(s), {item.Images.Count} image(s), {item.Documents.Count} document(s).",
            ct);

        item.Category = category;
        return Result<ItemDto>.Success(ToDto(item));
    }

    public async Task<Result> UpdateAsync(
        Guid id,
        string code,
        string name,
        string? description,
        string? barcode,
        Guid categoryId,
        string? unitOfMeasurement,
        decimal price,
        decimal costPrice,
        int stockQuantity,
        bool isActive,
        IReadOnlyList<ItemImageInput> images,
        IReadOnlyList<ItemDocumentInput> documents,
        IReadOnlyList<ItemVariantInput> variants,
        CancellationToken ct)
    {
        var item = await db.Items
            .Include(i => i.Images)
            .Include(i => i.Documents)
            .Include(i => i.Variants)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

        if (item is null)
        {
            return Result.Failure("Item not found.");
        }

        var trimmedCode = code.Trim();
        var trimmedName = name.Trim();

        if (string.IsNullOrWhiteSpace(trimmedCode))
        {
            return Result.Failure("Item code is required.");
        }

        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            return Result.Failure("Item name is required.");
        }

        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == categoryId, ct);
        if (category is null)
        {
            return Result.Failure("Selected category does not exist.");
        }

        var codeExists = await db.Items.AnyAsync(i => i.Code == trimmedCode && i.Id != id, ct);
        if (codeExists)
        {
            return Result.Failure($"Item with code '{trimmedCode}' already exists.");
        }

        item.Code = trimmedCode;
        item.Name = trimmedName;
        item.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        item.Barcode = string.IsNullOrWhiteSpace(barcode) ? null : barcode.Trim();
        item.CategoryId = categoryId;
        item.UnitOfMeasurement = ParseUnitOfMeasurement(unitOfMeasurement, category.UnitOfMeasurement);
        item.Price = price;
        item.CostPrice = costPrice;
        item.StockQuantity = stockQuantity;
        item.IsActive = isActive;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        // Replace Images
        db.ItemImages.RemoveRange(item.Images);
        item.Images = images.Select((img, index) => new ItemImage
        {
            ItemId = item.Id,
            Url = img.Url,
            FileName = img.FileName,
            Caption = img.Caption,
            IsPrimary = img.IsPrimary,
            SortOrder = img.SortOrder > 0 ? img.SortOrder : index + 1,
            UploadedAt = DateTimeOffset.UtcNow
        }).ToList();

        if (item.Images.Count > 0 && !item.Images.Any(i => i.IsPrimary))
        {
            item.Images[0].IsPrimary = true;
        }

        // Replace Documents
        db.ItemDocuments.RemoveRange(item.Documents);
        item.Documents = documents.Select(doc => new ItemDocument
        {
            ItemId = item.Id,
            Url = doc.Url,
            FileName = doc.FileName,
            DocumentType = doc.DocumentType,
            FileSizeBytes = doc.FileSizeBytes,
            Description = doc.Description,
            UploadedAt = DateTimeOffset.UtcNow
        }).ToList();

        // Replace Variants
        db.ItemVariants.RemoveRange(item.Variants);
        item.Variants = variants.Select(v => new ItemVariant
        {
            ItemId = item.Id,
            Sku = string.IsNullOrWhiteSpace(v.Sku) ? $"{trimmedCode}-{Guid.NewGuid().ToString()[..6].ToUpper()}" : v.Sku.Trim(),
            Name = v.Name?.Trim() ?? string.Empty,
            Barcode = string.IsNullOrWhiteSpace(v.Barcode) ? null : v.Barcode.Trim(),
            AttributesJson = string.IsNullOrWhiteSpace(v.AttributesJson) ? "{}" : v.AttributesJson,
            Price = v.Price,
            CostPrice = v.CostPrice,
            StockQuantity = v.StockQuantity,
            IsActive = v.IsActive
        }).ToList();

        await db.SaveChangesAsync(ct);

        await activityLog.LogAsync(
            EntityTypes.Item,
            item.Id.ToString(),
            "Updated",
            $"Item {item.Code} ({item.Name}) updated; price={item.Price:C}, {item.Variants.Count} variant(s), {item.Images.Count} image(s), {item.Documents.Count} document(s).",
            ct);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct)
    {
        var item = await db.Items.FirstOrDefaultAsync(i => i.Id == id, ct);
        if (item is null)
        {
            return Result.Failure("Item not found.");
        }

        item.IsDeleted = true;
        item.IsActive = false;
        item.DeletedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        await activityLog.LogAsync(
            EntityTypes.Item,
            item.Id.ToString(),
            "Deleted",
            $"Item {item.Code} ({item.Name}) soft-deleted.",
            ct);

        return Result.Success();
    }

    private static ItemDto ToDto(Item item)
    {
        return new ItemDto
        {
            Id = item.Id,
            Code = item.Code,
            Name = item.Name,
            Description = item.Description,
            Barcode = item.Barcode,
            CategoryId = item.CategoryId,
            CategoryName = item.Category?.Name ?? string.Empty,
            UnitOfMeasurement = item.UnitOfMeasurement.ToString(),
            Price = item.Price,
            CostPrice = item.CostPrice,
            StockQuantity = item.StockQuantity,
            IsActive = item.IsActive,
            CreatedAt = item.CreatedAt,
            Images = item.Images.OrderBy(img => img.SortOrder).Select(img => new ItemImageDto
            {
                Id = img.Id,
                ItemId = img.ItemId,
                Url = img.Url,
                FileName = img.FileName,
                Caption = img.Caption,
                IsPrimary = img.IsPrimary,
                SortOrder = img.SortOrder,
                UploadedAt = img.UploadedAt
            }).ToList(),
            Documents = item.Documents.OrderByDescending(doc => doc.UploadedAt).Select(doc => new ItemDocumentDto
            {
                Id = doc.Id,
                ItemId = doc.ItemId,
                Url = doc.Url,
                FileName = doc.FileName,
                DocumentType = doc.DocumentType,
                FileSizeBytes = doc.FileSizeBytes,
                Description = doc.Description,
                UploadedAt = doc.UploadedAt
            }).ToList(),
            Variants = item.Variants.Select(v => new ItemVariantDto
            {
                Id = v.Id,
                ItemId = v.ItemId,
                Sku = v.Sku,
                Name = v.Name,
                Barcode = v.Barcode,
                AttributesJson = v.AttributesJson,
                Price = v.Price,
                CostPrice = v.CostPrice,
                StockQuantity = v.StockQuantity,
                IsActive = v.IsActive
            }).ToList()
        };
    }

    private static UnitOfMeasurement ParseUnitOfMeasurement(string? uom, UnitOfMeasurement fallback)
    {
        if (!string.IsNullOrWhiteSpace(uom) && Enum.TryParse<UnitOfMeasurement>(uom, ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        return fallback;
    }
}
