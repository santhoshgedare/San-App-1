using IdentityHub.Application.Common.Models;

namespace IdentityHub.Application.Common.Interfaces;

public interface IItemService
{
    Task<IReadOnlyList<ItemDto>> GetAllAsync(CancellationToken ct);
    Task<ItemDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Guid?> GetSellerIdAsync(Guid id, CancellationToken ct);
    Task<PagedResult<ItemDto>> GetPagedAsync(ItemListQuery query, CancellationToken ct);
    Task<IReadOnlyList<ItemDto>> GetSimilarAsync(Guid id, int take, CancellationToken ct);
    Task<Result<ItemDto>> CreateAsync(
        string code,
        string name,
        string? description,
        string? barcode,
        Guid categoryId,
        Guid sellerId,
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
        Guid sellerId,
        string? unitOfMeasurement,
        decimal price,
        decimal costPrice,
        int stockQuantity,
        bool isActive,
        IReadOnlyList<ItemImageInput> images,
        IReadOnlyList<ItemDocumentInput> documents,
        IReadOnlyList<ItemVariantInput> variants,
        CancellationToken ct);
    Task<Result> SetApprovalAsync(Guid id, IdentityHub.Domain.Enums.ItemApprovalStatus status, string? note, CancellationToken ct);
    Task<Result> SetActiveAsync(Guid id, bool isActive, CancellationToken ct);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct);
}
