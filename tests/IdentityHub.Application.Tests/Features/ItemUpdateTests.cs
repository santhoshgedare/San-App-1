using FluentAssertions;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Domain.Entities;
using IdentityHub.Infrastructure.Persistence;
using IdentityHub.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IdentityHub.Application.Tests.Features;

public sealed class ItemUpdateTests
{
    [Fact]
    public async Task Updates_children_by_id_and_keeps_order_linked_variants()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        await using var db = new AppDbContext(options);

        var category = new Category { Name = "Category" };
        var image = new ItemImage { Url = "old-image", IsPrimary = true };
        var document = new ItemDocument { Url = "old-document", FileName = "spec.pdf" };
        var variant = new ItemVariant { Sku = "ITEM-A", Name = "Option A", AttributesJson = "{}", Price = 25m, StockQuantity = 2 };
        var item = new Item
        {
            Code = "ITEM-1",
            Name = "Sample",
            Category = category,
            CategoryId = category.Id,
            Price = 25m,
            StockQuantity = 5,
            Images = [image],
            Documents = [document],
            Variants = [variant]
        };
        var order = new Order
        {
            OrderNumber = "ORD-TEST-1",
            CustomerName = "Customer",
            CustomerEmail = "customer@example.com",
            CustomerPhone = "555-0100",
            ShippingAddress = "Address",
            Items =
            [
                new OrderItem
                {
                    Item = item,
                    ItemVariant = variant,
                    ItemCode = item.Code,
                    ItemName = item.Name,
                    VariantSku = variant.Sku,
                    VariantName = variant.Name,
                    UnitPrice = variant.Price,
                    Quantity = 1,
                    TotalPrice = variant.Price
                }
            ]
        };
        db.Categories.Add(category);
        db.Items.Add(item);
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var result = await new ItemService(db, new NoOpActivityLogService()).UpdateAsync(
            item.Id,
            "ITEM-1",
            "Sample Updated",
            null,
            null,
            category.Id,
            "Piece",
            30m,
            10m,
            5,
            true,
            [new ItemImageInput(image.Id, "new-image", null, null, true, 1)],
            [new ItemDocumentInput(document.Id, "new-document", "spec.pdf", null, 100, null)],
            [],
            CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        var updatedImage = await db.ItemImages.SingleAsync();
        var updatedDocument = await db.ItemDocuments.SingleAsync();
        var retainedVariant = await db.ItemVariants.SingleAsync();
        updatedImage.Id.Should().Be(image.Id);
        updatedImage.Url.Should().Be("new-image");
        updatedDocument.Id.Should().Be(document.Id);
        updatedDocument.Url.Should().Be("new-document");
        retainedVariant.Id.Should().Be(variant.Id);
        retainedVariant.IsActive.Should().BeFalse();
        (await db.OrderItems.SingleAsync()).ItemVariantId.Should().Be(variant.Id);
    }

    private sealed class NoOpActivityLogService : IActivityLogService
    {
        public Task LogAsync(string entityType, string entityId, string action, string? details, CancellationToken ct) => Task.CompletedTask;
        public Task<IReadOnlyList<ActivityLogDto>> GetForEntityAsync(string entityType, string entityId, CancellationToken ct) => throw new NotSupportedException();
        public Task<PagedResult<ActivityLogDto>> GetPagedAsync(ActivityLogQuery query, CancellationToken ct) => throw new NotSupportedException();
    }
}