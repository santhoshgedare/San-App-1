using System.Text.Json;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Domain.Entities;
using IdentityHub.Domain.Enums;
using IdentityHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityHub.Infrastructure.Services;

public sealed class ReviewService(AppDbContext db) : IReviewService
{
    private const int MaxImages = 4;
    private const int MaxImageChars = 900_000;
    private static readonly string[] AllowedImagePrefixes =
        ["data:image/jpeg;base64,", "data:image/png;base64,", "data:image/webp;base64,"];

    public async Task<ItemReviewSummaryDto> GetForItemAsync(Guid itemId, int take, CancellationToken ct)
    {
        take = take is < 1 or > 50 ? 20 : take;
        var query = db.ItemReviews.AsNoTracking().Where(r => r.ItemId == itemId);

        var counts = await query.GroupBy(r => r.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        var total = counts.Sum(c => c.Count);
        var average = total == 0 ? 0 : counts.Sum(c => c.Rating * c.Count) / (double)total;

        var rows = await query.OrderByDescending(r => r.CreatedAt).Take(take).ToListAsync(ct);

        return new ItemReviewSummaryDto
        {
            AverageRating = Math.Round(average, 1),
            TotalCount = total,
            Distribution = Enumerable.Range(1, 5).ToDictionary(n => n, n => counts.FirstOrDefault(c => c.Rating == n)?.Count ?? 0),
            Reviews = rows.Select(ToDto).ToList()
        };
    }

    public async Task<IReadOnlyList<ItemReviewDto>> GetForOrderAsync(Guid orderId, Guid userId, CancellationToken ct)
    {
        var rows = await db.ItemReviews.AsNoTracking()
            .Where(r => r.OrderId == orderId && r.UserId == userId)
            .ToListAsync(ct);
        return rows.Select(ToDto).ToList();
    }

    public async Task<Result<ItemReviewDto>> CreateAsync(
        Guid userId,
        Guid orderItemId,
        int rating,
        string? title,
        string? comment,
        IReadOnlyList<string> images,
        CancellationToken ct)
    {
        if (rating is < 1 or > 5)
        {
            return Result<ItemReviewDto>.Failure("Rating must be between 1 and 5.");
        }

        var trimmedTitle = title?.Trim();
        var trimmedComment = comment?.Trim();
        if (trimmedTitle?.Length > 150)
        {
            return Result<ItemReviewDto>.Failure("Title must be 150 characters or fewer.");
        }

        if (trimmedComment?.Length > 2000)
        {
            return Result<ItemReviewDto>.Failure("Review must be 2000 characters or fewer.");
        }

        if (images.Count > MaxImages)
        {
            return Result<ItemReviewDto>.Failure($"You can attach up to {MaxImages} photos.");
        }

        foreach (var image in images)
        {
            if (image.Length > MaxImageChars || !AllowedImagePrefixes.Any(p => image.StartsWith(p, StringComparison.Ordinal)))
            {
                return Result<ItemReviewDto>.Failure("Photos must be JPEG, PNG or WebP and under about 600 KB each.");
            }
        }

        var orderItem = await db.OrderItems.Include(oi => oi.Order).FirstOrDefaultAsync(oi => oi.Id == orderItemId, ct);
        if (orderItem?.Order is null || orderItem.Order.CustomerId != userId)
        {
            return Result<ItemReviewDto>.Failure("Order item not found.");
        }

        if (orderItem.Order.Status != OrderStatus.Delivered)
        {
            return Result<ItemReviewDto>.Failure("You can review an item once the order has been delivered.");
        }

        if (await db.ItemReviews.AnyAsync(r => r.OrderItemId == orderItemId, ct))
        {
            return Result<ItemReviewDto>.Failure("You have already reviewed this item.");
        }

        var review = new ItemReview
        {
            ItemId = orderItem.ItemId,
            OrderId = orderItem.OrderId,
            OrderItemId = orderItem.Id,
            UserId = userId,
            ReviewerName = MaskName(orderItem.Order.CustomerName),
            Rating = rating,
            Title = string.IsNullOrWhiteSpace(trimmedTitle) ? null : trimmedTitle,
            Comment = string.IsNullOrWhiteSpace(trimmedComment) ? null : trimmedComment,
            ImagesJson = images.Count == 0 ? null : JsonSerializer.Serialize(images)
        };

        db.ItemReviews.Add(review);
        await db.SaveChangesAsync(ct);
        return Result<ItemReviewDto>.Success(ToDto(review));
    }

    // Shows "Priya S." instead of a full name.
    private static string MaskName(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length switch
        {
            0 => "Customer",
            1 => parts[0],
            _ => $"{parts[0]} {char.ToUpperInvariant(parts[^1][0])}."
        };
    }

    private static ItemReviewDto ToDto(ItemReview r) => new()
    {
        Id = r.Id,
        ItemId = r.ItemId,
        OrderItemId = r.OrderItemId,
        ReviewerName = r.ReviewerName,
        Rating = r.Rating,
        Title = r.Title,
        Comment = r.Comment,
        Images = string.IsNullOrEmpty(r.ImagesJson) ? [] : JsonSerializer.Deserialize<List<string>>(r.ImagesJson) ?? [],
        CreatedAt = r.CreatedAt
    };
}