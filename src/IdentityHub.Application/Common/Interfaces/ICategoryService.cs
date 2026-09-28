using IdentityHub.Application.Common.Models;

namespace IdentityHub.Application.Common.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct);
    Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<PagedResult<CategoryDto>> GetPagedAsync(CategoryListQuery query, CancellationToken ct);
    Task<Result<CategoryDto>> CreateAsync(
        string name,
        string? description,
        bool isActive,
        string? unitOfMeasurement,
        IReadOnlyList<CategoryVariantDefinitionInput> variantDefinitions,
        CancellationToken ct);
    Task<Result> UpdateAsync(
        Guid id,
        string name,
        string? description,
        bool isActive,
        string? unitOfMeasurement,
        IReadOnlyList<CategoryVariantDefinitionInput> variantDefinitions,
        CancellationToken ct);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct);
}
