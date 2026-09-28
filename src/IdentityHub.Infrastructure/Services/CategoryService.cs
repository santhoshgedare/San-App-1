using System.Text.Json;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Domain.Constants;
using IdentityHub.Domain.Entities;
using IdentityHub.Domain.Enums;
using IdentityHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityHub.Infrastructure.Services;

public sealed class CategoryService(AppDbContext db, IActivityLogService activityLog) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct)
    {
        var categories = await db.Categories
            .Include(c => c.VariantDefinitions)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

        return categories.Select(ToDto).ToList();
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var category = await db.Categories
            .Include(c => c.VariantDefinitions)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        return category is null ? null : ToDto(category);
    }

    public async Task<PagedResult<CategoryDto>> GetPagedAsync(CategoryListQuery query, CancellationToken ct)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var categoriesQuery = db.Categories
            .Include(c => c.VariantDefinitions)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            categoriesQuery = categoriesQuery.Where(c =>
                c.Name.Contains(term) ||
                (c.Description != null && c.Description.Contains(term)));
        }

        if (query.IsActive.HasValue)
        {
            categoriesQuery = categoriesQuery.Where(c => c.IsActive == query.IsActive.Value);
        }

        var totalCount = await categoriesQuery.CountAsync(ct);
        var pageOfCategories = await categoriesQuery
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<CategoryDto>
        {
            Items = pageOfCategories.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Result<CategoryDto>> CreateAsync(
        string name,
        string? description,
        bool isActive,
        string? unitOfMeasurement,
        IReadOnlyList<CategoryVariantDefinitionInput> variantDefinitions,
        CancellationToken ct)
    {
        var trimmedName = name.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            return Result<CategoryDto>.Failure("Category name is required.");
        }

        var uom = ParseUnitOfMeasurement(unitOfMeasurement);

        var category = new Category
        {
            Name = trimmedName,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            IsActive = isActive,
            UnitOfMeasurement = uom,
            CreatedAt = DateTimeOffset.UtcNow,
            VariantDefinitions = variantDefinitions.Select(v => new CategoryVariantDefinition
            {
                Name = string.IsNullOrWhiteSpace(v.Name) ? v.Type : v.Name.Trim(),
                Type = ParseCategoryVariantType(v.Type),
                ValuesJson = JsonSerializer.Serialize(v.Values ?? []),
                IsRequired = v.IsRequired
            }).ToList()
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);

        await activityLog.LogAsync(
            EntityTypes.Category,
            category.Id.ToString(),
            "Created",
            $"Category {category.Name} created with {category.VariantDefinitions.Count} variant definition(s).",
            ct);

        return Result<CategoryDto>.Success(ToDto(category));
    }

    public async Task<Result> UpdateAsync(
        Guid id,
        string name,
        string? description,
        bool isActive,
        string? unitOfMeasurement,
        IReadOnlyList<CategoryVariantDefinitionInput> variantDefinitions,
        CancellationToken ct)
    {
        var category = await db.Categories
            .Include(c => c.VariantDefinitions)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (category is null)
        {
            return Result.Failure("Category not found.");
        }

        var trimmedName = name.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            return Result.Failure("Category name is required.");
        }

        category.Name = trimmedName;
        category.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        category.IsActive = isActive;
        category.UnitOfMeasurement = ParseUnitOfMeasurement(unitOfMeasurement);
        category.UpdatedAt = DateTimeOffset.UtcNow;

        db.CategoryVariantDefinitions.RemoveRange(category.VariantDefinitions);
        category.VariantDefinitions = variantDefinitions.Select(v => new CategoryVariantDefinition
        {
            CategoryId = category.Id,
            Name = string.IsNullOrWhiteSpace(v.Name) ? v.Type : v.Name.Trim(),
            Type = ParseCategoryVariantType(v.Type),
            ValuesJson = JsonSerializer.Serialize(v.Values ?? []),
            IsRequired = v.IsRequired
        }).ToList();

        await db.SaveChangesAsync(ct);

        await activityLog.LogAsync(
            EntityTypes.Category,
            category.Id.ToString(),
            "Updated",
            $"Category {category.Name} updated; active={category.IsActive}, UoM={category.UnitOfMeasurement}.",
            ct);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (category is null)
        {
            return Result.Failure("Category not found.");
        }

        category.IsDeleted = true;
        category.IsActive = false;
        category.DeletedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        await activityLog.LogAsync(
            EntityTypes.Category,
            category.Id.ToString(),
            "Deleted",
            $"Category {category.Name} soft-deleted.",
            ct);

        return Result.Success();
    }

    private static CategoryDto ToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            UnitOfMeasurement = category.UnitOfMeasurement.ToString(),
            CreatedAt = category.CreatedAt,
            VariantDefinitions = category.VariantDefinitions.Select(v => new CategoryVariantDefinitionDto
            {
                Id = v.Id,
                Name = v.Name,
                Type = v.Type.ToString(),
                Values = TryDeserializeValues(v.ValuesJson),
                IsRequired = v.IsRequired
            }).ToList()
        };
    }

    private static IReadOnlyList<string> TryDeserializeValues(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static UnitOfMeasurement ParseUnitOfMeasurement(string? uom)
    {
        if (Enum.TryParse<UnitOfMeasurement>(uom, ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        return UnitOfMeasurement.Piece;
    }

    private static CategoryVariantType ParseCategoryVariantType(string? type)
    {
        if (Enum.TryParse<CategoryVariantType>(type, ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        return CategoryVariantType.Custom;
    }
}
