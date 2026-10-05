using IdentityHub.Application.Common.Models;

namespace IdentityHub.Application.Common.Interfaces;

public interface IItemService
{
    Task<IReadOnlyList<ItemDto>> GetAllAsync(CancellationToken ct);
    Task<ItemDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Guid?> GetSellerIdAsync(Guid id, CancellationToken ct);
    Task AssignSellerAsync(Guid id, Guid? sellerId, CancellationToken ct);
    Task<PagedResult<ItemDto>> GetPagedAsync(ItemListQuery query, CancellationToken ct);
    Task<IReadOnlyList<ItemDto>> GetSimilarAsync(Guid id, int take, CancellationToken ct);
    Task<Result<ItemDto>> CreateAsync(
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
        CancellationToken ct);
    Task<Result> UpdateAsync(
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
        CancellationToken ct);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct);
}
