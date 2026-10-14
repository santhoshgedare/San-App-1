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

        var order = await db.Orders.SingleAsync();
        order.Status = OrderStatus.Shipped;
        order.ShippingFeeConfirmed = true;
        order.PaymentStatus = PaymentStatus.Paid;
        await db.SaveChangesAsync();
        var cancellation = await service.UpdateStatusAsync(created.Data!.Id, OrderStatus.Cancelled, null, null);

        cancellation.Succeeded.Should().BeFalse();
        (await db.Items.SingleAsync()).StockQuantity.Should().Be(1);
    }

    [Theory]
    [InlineData(ApprovalStatus.Pending, false)]
    [InlineData(ApprovalStatus.Rejected, false)]
    [InlineData(ApprovalStatus.Approved, true)]
    public async Task Order_can_only_leave_pending_after_approval(ApprovalStatus approval, bool allowed)
    {
        await using var db = CreateDbContext();
        var item = new Item { Code = "ITEM-6", Name = "Sample", Price = 25m, StockQuantity = 3, IsActive = true };
        db.Items.Add(item);
        await db.SaveChangesAsync();
        var service = CreateOrderService(db);
        var created = await service.CreateAsync(
            null, "Customer", "customer@example.com", "555-0100", "Address", null, null,
            PaymentMethod.UpiQr, "REF-6", null, [CreateOrderLine(item, quantity: 1)]);
        db.Approvals.Add(new Approval
        {
            EntityType = IdentityHub.Domain.Constants.EntityTypes.Order,
            EntityId = created.Data!.Id.ToString(),
            Status = approval,
        });
        await db.SaveChangesAsync();

        (await service.SetShippingFeeAsync(created.Data.Id, 10m, null)).Succeeded.Should().Be(allowed);
        (await service.GetByIdAsync(created.Data.Id))!.ApprovalStatus.Should().Be(approval.ToString());
        if (!allowed)
        {
            (await service.UpdateStatusAsync(created.Data.Id, OrderStatus.Confirmed, null, null)).Succeeded.Should().BeFalse();
            (await service.UpdateStatusAsync(created.Data.Id, OrderStatus.Cancelled, null, null)).Succeeded.Should().BeTrue();
        }
    }

    [Fact]
    public async Task Order_moves_through_charge_acceptance_advance_fulfilment_and_final_payment_in_order()
    {
        await using var db = CreateDbContext();
        var item = new Item { Code = "ITEM-7", Name = "Sample", Price = 100m, StockQuantity = 5, IsActive = true };
        db.Items.Add(item);
        await db.SaveChangesAsync();
        var service = CreateOrderService(db);
        var buyer = Guid.NewGuid();
        var created = await service.CreateAsync(
            buyer, "Customer", "customer@example.com", "555-0100", "Address", null, null,
            PaymentMethod.UpiQr, null, null, [CreateOrderLine(item, quantity: 1, submittedPrice: 100m)]);
        var id = created.Data!.Id;
        (await db.Orders.SingleAsync()).CustomRequestId = Guid.NewGuid();
        await db.SaveChangesAsync();

        (await service.UpdateStatusAsync(id, OrderStatus.Confirmed, null, null, 30m)).Succeeded.Should().BeFalse("no delivery charge yet");
        (await service.AcceptChargesAsync(id, buyer)).Succeeded.Should().BeFalse("no delivery charge yet");

        (await service.SetShippingFeeAsync(id, 20m, null)).Succeeded.Should().BeTrue();
        (await service.UpdateStatusAsync(id, OrderStatus.Confirmed, null, null, 30m)).Succeeded.Should().BeFalse("buyer has not accepted");
        (await service.AcceptChargesAsync(id, Guid.NewGuid())).Succeeded.Should().BeFalse("not the buyer");
        (await service.AcceptChargesAsync(id, buyer)).Succeeded.Should().BeTrue();

        (await service.UpdateStatusAsync(id, OrderStatus.Confirmed, null, null, 999m)).Succeeded.Should().BeFalse("advance exceeds total");
        (await service.UpdateStatusAsync(id, OrderStatus.Confirmed, null, null, 30m)).Succeeded.Should().BeTrue();
        var confirmed = (await service.GetByIdAsync(id))!;
        confirmed.AdvanceAmount.Should().Be(30m);
        confirmed.PaymentStage.Should().Be("Advance");
        confirmed.AmountDue.Should().Be(30m);

        (await service.UpdateStatusAsync(id, OrderStatus.Processing, null, null)).Succeeded.Should().BeFalse("advance unpaid");
        (await service.SetShippingFeeAsync(id, 25m, null)).Succeeded.Should().BeFalse("charge locked once confirmed");
        (await service.SubmitPaymentDetailsAsync(id, "ADV-1", null)).Succeeded.Should().BeTrue();
        (await service.UpdatePaymentStatusAsync(id, PaymentStatus.Paid, null, null)).Succeeded.Should().BeTrue();
        (await service.UpdateStatusAsync(id, OrderStatus.Cancelled, null, null)).Succeeded.Should().BeFalse("advance must be refunded first");

        (await service.UpdateStatusAsync(id, OrderStatus.Processing, null, null)).Succeeded.Should().BeTrue();
        var processing = (await service.GetByIdAsync(id))!;
        processing.PaymentStage.Should().Be("Final");
        processing.AmountDue.Should().Be(90m);
        processing.PaymentStatus.Should().Be("Pending");

        (await service.UpdateStatusAsync(id, OrderStatus.Shipped, "TRK", "Carrier")).Succeeded.Should().BeFalse("final payment due");
        (await service.UpdatePaymentStatusAsync(id, PaymentStatus.Paid, "FINAL-1", null)).Succeeded.Should().BeTrue();
        (await service.UpdateStatusAsync(id, OrderStatus.Shipped, "TRK", "Carrier")).Succeeded.Should().BeTrue();
        var saved = await db.Orders.SingleAsync();
        saved.AdvanceReferenceNumber.Should().Be("ADV-1");
        saved.PaymentReferenceNumber.Should().Be("FINAL-1");
    }

    [Fact]
    public async Task Advance_only_payment_can_be_refunded_in_full_then_order_cancelled()
    {
        await using var db = CreateDbContext();
        var item = new Item { Code = "ITEM-8", Name = "Sample", Price = 100m, StockQuantity = 5, IsActive = true };
        db.Items.Add(item);
        await db.SaveChangesAsync();
        var service = CreateOrderService(db);
        var buyer = Guid.NewGuid();
        var created = await service.CreateAsync(
            buyer, "Customer", "customer@example.com", "555-0100", "Address", null, null,
            PaymentMethod.UpiQr, null, null, [CreateOrderLine(item, quantity: 1, submittedPrice: 100m)]);
        var id = created.Data!.Id;
        (await db.Orders.SingleAsync()).CustomRequestId = Guid.NewGuid();
        await db.SaveChangesAsync();
        await service.SetShippingFeeAsync(id, 0m, null);
        await service.AcceptChargesAsync(id, buyer);
        await service.UpdateStatusAsync(id, OrderStatus.Confirmed, null, null, 40m);
        await service.SubmitPaymentDetailsAsync(id, "ADV-2", null);
        await service.UpdatePaymentStatusAsync(id, PaymentStatus.Paid, null, null);

        (await service.RecordRefundAsync(id, 100m, "REF-1", null)).Succeeded.Should().BeFalse("only the advance was received");
        (await service.RecordRefundAsync(id, 40m, "REF-1", null)).Succeeded.Should().BeTrue();
        (await service.UpdateStatusAsync(id, OrderStatus.Cancelled, null, null)).Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task In_stock_order_requires_full_payment_before_fulfilment_even_if_advance_is_sent()
    {
        await using var db = CreateDbContext();
        var item = new Item { Code = "ITEM-9", Name = "Sample", Price = 100m, StockQuantity = 5, IsActive = true };
        db.Items.Add(item);
        await db.SaveChangesAsync();
        var service = CreateOrderService(db);
        var buyer = Guid.NewGuid();
        var created = await service.CreateAsync(
            buyer, "Customer", "customer@example.com", "555-0100", "Address", null, null,
            PaymentMethod.UpiQr, null, null, [CreateOrderLine(item, quantity: 1, submittedPrice: 100m)]);
        var id = created.Data!.Id;
        await service.SetShippingFeeAsync(id, 0m, null);
        await service.AcceptChargesAsync(id, buyer);
        (await service.UpdateStatusAsync(id, OrderStatus.Confirmed, null, null, 30m)).Succeeded.Should().BeTrue();

        var confirmed = (await service.GetByIdAsync(id))!;
        confirmed.AdvanceAmount.Should().Be(0m);
        confirmed.PaymentStage.Should().Be("Final");
        confirmed.AmountDue.Should().Be(100m);
        (await service.UpdateStatusAsync(id, OrderStatus.Processing, null, null)).Succeeded.Should().BeFalse("full payment unpaid");
        (await service.UpdatePaymentStatusAsync(id, PaymentStatus.Paid, "FULL-1", null)).Succeeded.Should().BeTrue();
        (await service.UpdateStatusAsync(id, OrderStatus.Processing, null, null)).Succeeded.Should().BeTrue();
    }
    [Fact]
    public async Task Delivery_charge_cannot_be_set_until_order_is_approved()
    {
        await using var db = CreateDbContext();
        var item = new Item { Code = "ITEM-9", Name = "Sample", Price = 25m, StockQuantity = 3, IsActive = true };
        db.Items.Add(item);
        await db.SaveChangesAsync();
        var service = CreateOrderService(db);
        var created = await service.CreateAsync(
            Guid.NewGuid(), "Customer", "customer@example.com", "555-0100", "Address", null, null,
            PaymentMethod.UpiQr, null, null, [CreateOrderLine(item, quantity: 1)]);
        db.Approvals.Add(new Approval { EntityType = IdentityHub.Domain.Constants.EntityTypes.Order, EntityId = created.Data!.Id.ToString(), Status = ApprovalStatus.Pending });
        await db.SaveChangesAsync();

        (await service.SetShippingFeeAsync(created.Data.Id, 10m, null)).Succeeded.Should().BeFalse();
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
        new(db, new NoOpActivityLogService(), new NoOpApprovalService(), new NoOpOrderNotifier());

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