using IdentityHub.Application.Common.Models;

namespace IdentityHub.Application.Common.Interfaces;

public interface IReviewService
{
    Task<ItemReviewSummaryDto> GetForItemAsync(Guid itemId, int take, CancellationToken ct);
    Task<IReadOnlyList<ItemReviewDto>> GetForOrderAsync(Guid orderId, Guid userId, CancellationToken ct);
    Task<Result<ItemReviewDto>> CreateAsync(
        Guid userId,
        Guid orderItemId,
        int rating,
        string? title,
        string? comment,
        IReadOnlyList<string> images,
        CancellationToken ct);
}