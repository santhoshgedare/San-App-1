using FluentAssertions;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Domain.Entities;
using IdentityHub.Domain.Enums;
using IdentityHub.Infrastructure.Persistence;
using IdentityHub.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IdentityHub.Application.Tests.Features;

public sealed class OrderInventoryTests
{
    [Fact]
    public async Task Rejects_order_when_requested_quantity_exceeds_stock()
    {
        await using var db = CreateDbContext();
        var item = new Item { Code = "ITEM-1", Name = "Sample", Price = 25m, StockQuantity = 1, IsActive = true };
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var result = await CreateOrderService(db).CreateAsync(
            null, "Customer", "customer@example.com", "555-0100", "Address", null, null,
            PaymentMethod.UpiQr, "REF-1", null, [CreateOrderLine(item, quantity: 2)]);

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("only 1 in stock"));
        (await db.Items.SingleAsync()).StockQuantity.Should().Be(1);
        (await db.Orders.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Reserves_stock_and_uses_database_price_when_order_is_created()
    {
        await using var db = CreateDbContext();
        var item = new Item { Code = "ITEM-2", Name = "Sample", Price = 25m, StockQuantity = 3, IsActive = true };
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var result = await CreateOrderService(db).CreateAsync(
            null, "Customer", "customer@example.com", "555-0100", "Address", null, null,
            PaymentMethod.UpiQr, "REF-2", null, [CreateOrderLine(item, quantity: 2, submittedPrice: 0.01m)]);

        result.Succeeded.Should().BeTrue();
        result.Data!.TotalAmount.Should().Be(50m);
        result.Data.Items.Single().UnitPrice.Should().Be(25m);
        (await db.Items.SingleAsync()).StockQuantity.Should().Be(1);
    }

    [Fact]
    public async Task Rejects_order_when_selected_variant_is_out_of_stock()
    {
        await using var db = CreateDbContext();
        var variant = new ItemVariant { Sku = "ITEM-4-A", Name = "Option A", Price = 30m, StockQuantity = 0, IsActive = true };
        var item = new Item
        {
            Code = "ITEM-4",
            Name = "Variant item",
            Price = 25m,
            StockQuantity = 10,
            IsActive = true,
            Variants = [variant]
        };
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var result = await CreateOrderService(db).CreateAsync(
            null, "Customer", "customer@example.com", "555-0100", "Address", null, null,
            PaymentMethod.UpiQr, "REF-4", null, [CreateOrderLine(item, quantity: 1, variant: variant)]);

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("only 0 in stock"));
        (await db.ItemVariants.SingleAsync()).StockQuantity.Should().Be(0);
        (await db.Orders.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Cancellation_restores_reserved_stock_only_once()
    {
        await using var db = CreateDbContext();
        var item = new Item { Code = "ITEM-3", Name = "Sample", Price = 25m, StockQuantity = 3, IsActive = true };
        db.Items.Add(item);
        await db.SaveChangesAsync();
        var service = CreateOrderService(db);

        var created = await service.CreateAsync(
            null, "Customer", "customer@example.com", "555-0100", "Address", null, null,
            PaymentMethod.UpiQr, "REF-3", null, [CreateOrderLine(item, quantity: 2)]);

        (await db.Items.SingleAsync()).StockQuantity.Should().Be(1);
        (await service.UpdateStatusAsync(created.Data!.Id, OrderStatus.Cancelled, null, null)).Succeeded.Should().BeTrue();
        (await db.Items.SingleAsync()).StockQuantity.Should().Be(3);
        (await service.UpdateStatusAsync(created.Data.Id, OrderStatus.Cancelled, null, null)).Succeeded.Should().BeTrue();
        (await db.Items.SingleAsync()).StockQuantity.Should().Be(3);
    }

    [Fact]
    public async Task Cannot_cancel_shipped_order_and_restore_its_stock()
    {
        await using var db = CreateDbContext();
        var item = new Item { Code = "ITEM-5", Name = "Sample", Price = 25m, StockQuantity = 3, IsActive = true };
        db.Items.Add(item);
        await db.SaveChangesAsync();
        var service = CreateOrderService(db);
        var created = await service.CreateAsync(
            null, "Customer", "customer@example.com", "555-0100", "Address", null, null,
            PaymentMethod.UpiQr, "REF-5", null, [CreateOrderLine(item, quantity: 2)]);

        (await service.UpdateStatusAsync(created.Data!.Id, OrderStatus.Shipped, null, null)).Succeeded.Should().BeTrue();
        var cancellation = await service.UpdateStatusAsync(created.Data.Id, OrderStatus.Cancelled, null, null);

        cancellation.Succeeded.Should().BeFalse();
        (await db.Items.SingleAsync()).StockQuantity.Should().Be(1);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AppDbContext(options);
    }

    private static OrderService CreateOrderService(AppDbContext db) =>
        new(db, new NoOpActivityLogService(), new NoOpApprovalService());

    private static OrderItemInput CreateOrderLine(
        Item item,
        int quantity,
        decimal submittedPrice = 25m,
        ItemVariant? variant = null) =>
        new(item.Id, variant?.Id, item.Code, item.Name, variant?.Sku, variant?.Name, variant?.AttributesJson, null, submittedPrice, quantity);

    private sealed class NoOpActivityLogService : IActivityLogService
    {
        public Task LogAsync(string entityType, string entityId, string action, string? details, CancellationToken ct) => Task.CompletedTask;
        public Task<IReadOnlyList<ActivityLogDto>> GetForEntityAsync(string entityType, string entityId, CancellationToken ct) => throw new NotSupportedException();
        public Task<PagedResult<ActivityLogDto>> GetPagedAsync(ActivityLogQuery query, CancellationToken ct) => throw new NotSupportedException();
    }

    private sealed class NoOpApprovalService : IApprovalService
    {
        public Task<ApprovalDto> RequestAsync(string entityType, string entityId, string title, string? details, CancellationToken ct) => Task.FromResult(new ApprovalDto());
        public Task<ApprovalDto> ApproveAsync(Guid approvalId, string? comment, CancellationToken ct) => throw new NotSupportedException();
        public Task<ApprovalDto> RejectAsync(Guid approvalId, string? comment, CancellationToken ct) => throw new NotSupportedException();
        public Task<ApprovalDto> ReassignStageApproverAsync(Guid approvalId, int stageIndex, Guid newApproverUserId, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyList<ApprovalDto>> GetForEntityAsync(string entityType, string entityId, CancellationToken ct) => throw new NotSupportedException();
        public Task<PagedResult<ApprovalDto>> GetPagedAsync(ApprovalQuery query, CancellationToken ct) => throw new NotSupportedException();
    }
}