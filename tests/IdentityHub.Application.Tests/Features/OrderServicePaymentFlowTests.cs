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

public sealed class OrderServicePaymentFlowTests
{
    [Fact]
    public async Task Refund_records_full_amount_and_reference_without_replacing_payment_reference()
    {
        await using var db = CreateDbContext();
        var order = CreatePaidOrder();
        await db.Orders.AddAsync(order);
        await db.SaveChangesAsync();
        var activityLog = new RecordingActivityLogService();
        var service = new OrderService(db, activityLog, new StubApprovalService());

        var result = await service.RecordRefundAsync(order.Id, order.TotalAmount, "REFUND-UTR-9", "Cancelled order");

        result.Succeeded.Should().BeTrue();
        var savedOrder = await db.Orders.SingleAsync();
        savedOrder.PaymentStatus.Should().Be(PaymentStatus.Refunded);
        savedOrder.PaymentReferenceNumber.Should().Be("PAYMENT-UTR-1");
        savedOrder.RefundAmount.Should().Be(order.TotalAmount);
        savedOrder.RefundReferenceNumber.Should().Be("REFUND-UTR-9");
        savedOrder.RefundNotes.Should().Be("Cancelled order");
        savedOrder.RefundedAt.Should().NotBeNull();
        activityLog.Actions.Should().ContainSingle().Which.Should().Be("RefundRecorded");
    }

    [Fact]
    public async Task Refund_is_rejected_when_payment_is_not_paid()
    {
        await using var db = CreateDbContext();
        var order = CreatePaidOrder();
        order.PaymentStatus = PaymentStatus.Pending;
        await db.Orders.AddAsync(order);
        await db.SaveChangesAsync();
        var activityLog = new RecordingActivityLogService();
        var service = new OrderService(db, activityLog, new StubApprovalService());

        var result = await service.RecordRefundAsync(order.Id, order.TotalAmount, "REFUND-UTR-9", null);

        result.Succeeded.Should().BeFalse();
        (await db.Orders.SingleAsync()).PaymentStatus.Should().Be(PaymentStatus.Pending);
        activityLog.Actions.Should().BeEmpty();
    }

    [Fact]
    public async Task Refund_is_rejected_when_amount_is_not_the_full_order_total()
    {
        await using var db = CreateDbContext();
        var order = CreatePaidOrder();
        await db.Orders.AddAsync(order);
        await db.SaveChangesAsync();
        var service = new OrderService(db, new RecordingActivityLogService(), new StubApprovalService());

        var result = await service.RecordRefundAsync(order.Id, order.TotalAmount - 1, "REFUND-UTR-9", null);

        result.Succeeded.Should().BeFalse();
        (await db.Orders.SingleAsync()).PaymentStatus.Should().Be(PaymentStatus.Paid);
    }

    [Fact]
    public async Task Customer_payment_details_cannot_be_changed_after_refund()
    {
        await using var db = CreateDbContext();
        var order = CreatePaidOrder();
        order.PaymentStatus = PaymentStatus.Refunded;
        await db.Orders.AddAsync(order);
        await db.SaveChangesAsync();
        var service = new OrderService(db, new RecordingActivityLogService(), new StubApprovalService());

        var result = await service.SubmitPaymentDetailsAsync(order.Id, "NEW-PAYMENT-UTR", null);

        result.Succeeded.Should().BeFalse();
        (await db.Orders.SingleAsync()).PaymentReferenceNumber.Should().Be("PAYMENT-UTR-1");
    }

    [Fact]
    public async Task Payment_cannot_be_submitted_or_confirmed_before_order_confirmation()
    {
        await using var db = CreateDbContext();
        var order = CreatePaidOrder();
        order.Status = OrderStatus.Pending;
        order.PaymentStatus = PaymentStatus.Pending;
        order.PaymentReferenceNumber = null;
        await db.Orders.AddAsync(order);
        await db.SaveChangesAsync();
        var activityLog = new RecordingActivityLogService();
        var service = new OrderService(db, activityLog, new StubApprovalService());

        var submittedDetails = await service.SubmitPaymentDetailsAsync(order.Id, "PAYMENT-UTR-2", null);
        var confirmedPayment = await service.UpdatePaymentStatusAsync(order.Id, PaymentStatus.Paid, "PAYMENT-UTR-2", null);

        submittedDetails.Succeeded.Should().BeFalse();
        confirmedPayment.Succeeded.Should().BeFalse();
        var savedOrder = await db.Orders.SingleAsync();
        savedOrder.Status.Should().Be(OrderStatus.Pending);
        savedOrder.PaymentStatus.Should().Be(PaymentStatus.Pending);
        savedOrder.PaymentReferenceNumber.Should().BeNull();
        activityLog.Actions.Should().BeEmpty();
    }

    [Fact]
    public async Task Profit_loss_report_accounts_for_refunds_and_order_time_costs()
    {
        await using var db = CreateDbContext();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var paidOrder = CreateFinancialOrder("ORD-REPORT-PAID", today, PaymentStatus.Paid, 100m, 20m, null, null);
        var refundedOrder = CreateFinancialOrder("ORD-REPORT-REFUND", today, PaymentStatus.Refunded, 50m, 15m, 50m, "REFUND-1");
        await db.Orders.AddRangeAsync(paidOrder, refundedOrder);
        await db.SaveChangesAsync();
        var service = new OrderService(db, new RecordingActivityLogService(), new StubApprovalService());
        var start = new DateTimeOffset(today.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        var end = new DateTimeOffset(today.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));

        var report = await service.GetProfitLossReportAsync(start, end, today, today);

        report.PaidOrderCount.Should().Be(1);
        report.RefundedOrderCount.Should().Be(1);
        report.GrossSales.Should().Be(150m);
        report.Refunds.Should().Be(50m);
        report.NetSales.Should().Be(100m);
        report.CostOfGoodsSold.Should().Be(70m);
        report.GrossProfitOrLoss.Should().Be(30m);
        report.IsComplete.Should().BeTrue();
    }

    [Fact]
    public async Task Profit_loss_report_withholds_result_when_historical_cost_is_missing()
    {
        await using var db = CreateDbContext();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var order = CreateFinancialOrder("ORD-REPORT-LEGACY", today, PaymentStatus.Paid, 100m, null, null, null);
        await db.Orders.AddAsync(order);
        await db.SaveChangesAsync();
        var service = new OrderService(db, new RecordingActivityLogService(), new StubApprovalService());
        var start = new DateTimeOffset(today.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        var end = new DateTimeOffset(today.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));

        var report = await service.GetProfitLossReportAsync(start, end, today, today);

        report.MissingCostLineCount.Should().Be(1);
        report.GrossProfitOrLoss.Should().BeNull();
        report.IsComplete.Should().BeFalse();
    }

    [Fact]
    public async Task Payment_cannot_be_confirmed_without_a_reference_or_downgraded_after_paid()
    {
        await using var db = CreateDbContext();
        var order = CreatePaidOrder();
        order.PaymentStatus = PaymentStatus.Pending;
        order.PaymentReferenceNumber = null;
        await db.Orders.AddAsync(order);
        await db.SaveChangesAsync();
        var service = new OrderService(db, new RecordingActivityLogService(), new StubApprovalService());

        var missingReference = await service.UpdatePaymentStatusAsync(order.Id, PaymentStatus.Paid, null, null);
        missingReference.Succeeded.Should().BeFalse();
        (await db.Orders.SingleAsync()).PaymentStatus.Should().Be(PaymentStatus.Pending);

        order.PaymentStatus = PaymentStatus.Paid;
        order.PaymentReferenceNumber = "PAYMENT-UTR-1";
        await db.SaveChangesAsync();
        var downgrade = await service.UpdatePaymentStatusAsync(order.Id, PaymentStatus.Pending, null, null);

        downgrade.Succeeded.Should().BeFalse();
        (await db.Orders.SingleAsync()).PaymentStatus.Should().Be(PaymentStatus.Paid);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AppDbContext(options);
    }

    private static Order CreatePaidOrder() => new()
    {
        OrderNumber = "ORD-TEST-001",
        CustomerName = "Test Customer",
        CustomerEmail = "customer@example.com",
        CustomerPhone = "1234567890",
        ShippingAddress = "1 Example Street",
        PaymentStatus = PaymentStatus.Paid,
        PaymentReferenceNumber = "PAYMENT-UTR-1",
        Status = OrderStatus.Confirmed,
        TotalAmount = 125.50m
    };

    private static Order CreateFinancialOrder(
        string orderNumber,
        DateOnly createdDate,
        PaymentStatus paymentStatus,
        decimal amount,
        decimal? unitCostPrice,
        decimal? refundAmount,
        string? refundReference) 
    {
        var category = new Category { Name = $"Category {orderNumber}" };
        var item = new Item
        {
            Code = orderNumber,
            Name = $"Item {orderNumber}",
            Category = category,
            CategoryId = category.Id,
            Price = amount,
            CostPrice = unitCostPrice ?? 0m
        };
        var order = CreatePaidOrder();
        order.OrderNumber = orderNumber;
        order.CreatedAt = new DateTimeOffset(createdDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        order.PaymentStatus = paymentStatus;
        order.TotalAmount = amount;
        order.RefundAmount = refundAmount;
        order.RefundReferenceNumber = refundReference;
        order.RefundedAt = paymentStatus == PaymentStatus.Refunded ? order.CreatedAt : null;
        order.Items.Add(new OrderItem
        {
            OrderId = order.Id,
            Order = order,
            ItemId = item.Id,
            Item = item,
            ItemCode = item.Code,
            ItemName = item.Name,
            UnitPrice = amount / 2,
            UnitCostPrice = unitCostPrice,
            Quantity = 2,
            TotalPrice = amount
        });
        return order;
    }

    private sealed class RecordingActivityLogService : IActivityLogService
    {
        public List<string> Actions { get; } = [];

        public Task LogAsync(string entityType, string entityId, string action, string? details, CancellationToken ct)
        {
            Actions.Add(action);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<ActivityLogDto>> GetForEntityAsync(string entityType, string entityId, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<ActivityLogDto>>([]);

        public Task<PagedResult<ActivityLogDto>> GetPagedAsync(ActivityLogQuery query, CancellationToken ct) =>
            Task.FromResult(new PagedResult<ActivityLogDto>());
    }

    private sealed class StubApprovalService : IApprovalService
    {
        public Task<ApprovalDto> RequestAsync(string entityType, string entityId, string title, string? details, CancellationToken ct) => throw new NotSupportedException();
        public Task<ApprovalDto> ApproveAsync(Guid approvalId, string? comment, CancellationToken ct) => throw new NotSupportedException();
        public Task<ApprovalDto> RejectAsync(Guid approvalId, string? comment, CancellationToken ct) => throw new NotSupportedException();
        public Task<ApprovalDto> ReassignStageApproverAsync(Guid approvalId, int stageIndex, Guid newApproverUserId, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyList<ApprovalDto>> GetForEntityAsync(string entityType, string entityId, CancellationToken ct) => throw new NotSupportedException();
        public Task<PagedResult<ApprovalDto>> GetPagedAsync(ApprovalQuery query, CancellationToken ct) => throw new NotSupportedException();
    }
}