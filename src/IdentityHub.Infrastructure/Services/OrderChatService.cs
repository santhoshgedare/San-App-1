using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Domain.Constants;
using IdentityHub.Domain.Entities;
using IdentityHub.Domain.Enums;
using IdentityHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityHub.Infrastructure.Services;

public sealed class OrderChatService(
    AppDbContext db,
    ICurrentUserService currentUser,
    IOrderNotifier notifier,
    IOrderChatLiveNotifier liveNotifier) : IOrderChatService
{
    private const int MaxLength = 1000;

    private static readonly OrderChatFaqDto[] Faqs =
    [
        new("status", "What is the status of my order?"),
        new("shipping", "When will my order ship?"),
        new("tracking", "How can I track my order?"),
        new("delivery-charge", "What is the delivery charge?"),
        new("payment", "How do I pay for my order?"),
        new("cancel", "Can I cancel my order?"),
        new("custom", "Can I ask for a change or custom request?"),
        new("problem", "I have a problem with my order"),
    ];

    public async Task<OrderChatDto?> GetAsync(Guid orderId, CancellationToken ct)
    {
        var access = await ResolveAsync(orderId, ct);
        if (access is null) return null;

        var me = currentUser.UserId;
        var messages = await db.OrderMessages
            .Where(m => m.OrderId == orderId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(500)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);

        var unseen = access.Role is "Buyer" or "Seller"
            ? messages.Where(m => m.ReadAt is null && m.SenderRole != "Auto" && m.SenderRole != access.Role).ToList()
            : [];
        if (unseen.Count > 0)
        {
            var now = DateTimeOffset.UtcNow;
            foreach (var m in unseen) m.ReadAt = now;
            await db.SaveChangesAsync(ct);
            var senderIds = access.Role == "Buyer" ? await StaffIdsAsync(access.Order.SellerId, ct) : access.Order.CustomerId is { } c ? [c] : [];
            foreach (var senderId in senderIds) await liveNotifier.NotifyAsync(senderId, orderId, ct);
        }

        return new OrderChatDto
        {
            MyRole = access.Role,
            CanPost = access.CanPost,
            IsClosed = access.Closed,
            SellerName = access.SellerName,
            Messages = messages.Select(m => new OrderChatMessageDto(m.Id, m.SenderRole, m.SenderName, m.Body, m.CreatedAt, m.SenderUserId != null && m.SenderUserId == me && m.SenderRole == access.Role, m.ReadAt)).ToList(),
            Faqs = access.Role == "Buyer" && access.CanPost ? Faqs : [],
        };
    }

    public async Task<IReadOnlyList<OrderChatNotificationDto>> GetNotificationsAsync(CancellationToken ct)
    {
        var me = currentUser.UserId;
        if (me is null) return [];

        return await db.OrderMessages
            .AsNoTracking()
            .Where(m => m.SenderRole != "Auto"
                && (m.Order!.CustomerId == me || m.Order.Seller!.UserId == me
                    || db.SellerEmployees.Any(e => e.UserId == me && e.SellerId == m.Order.SellerId)))
            .GroupBy(m => m.OrderId)
            .Select(group => group
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new OrderChatNotificationDto(
                    m.OrderId,
                    m.Order!.OrderNumber,
                    m.Id,
                    m.SenderRole,
                    m.SenderName,
                    m.Body,
                    m.CreatedAt,
                    m.SenderUserId == me))
                .First())
            .ToListAsync(ct);
    }

    public async Task<string?> SendAsync(Guid orderId, string? body, string? faqId, CancellationToken ct)
    {
        var access = await ResolveAsync(orderId, ct);
        if (access is null) return "Order not found.";
        if (!access.CanPost) return access.Closed ? "This conversation is closed because the order is " + access.Order.Status.ToString().ToLowerInvariant() + "." : "You can only read this conversation.";

        string text;
        string? autoAnswer = null;
        if (!string.IsNullOrWhiteSpace(faqId))
        {
            var faq = Faqs.FirstOrDefault(f => f.Id == faqId);
            if (faq is null || access.Role != "Buyer") return "Unknown question.";
            text = faq.Question;
            autoAnswer = Answer(faq.Id, access.Order, access.SellerName);
        }
        else
        {
            text = (body ?? string.Empty).Trim();
            if (text.Length == 0) return "Please type a message.";
            if (text.Length > MaxLength) return $"Messages can be at most {MaxLength} characters.";
        }

        var senderName = access.Role == "Buyer" ? access.Order.CustomerName : access.SellerName;
        db.OrderMessages.Add(new OrderMessage { OrderId = orderId, SenderUserId = currentUser.UserId, SenderRole = access.Role, SenderName = senderName, Body = text });
        if (autoAnswer is not null)
        {
            db.OrderMessages.Add(new OrderMessage { OrderId = orderId, SenderRole = "Auto", SenderName = "SRIVIDIKA Assistant", Body = autoAnswer, CreatedAt = DateTimeOffset.UtcNow.AddMilliseconds(5) });
        }

        await db.SaveChangesAsync(ct);
        var recipients = access.Role == "Buyer" ? await StaffIdsAsync(access.Order.SellerId, ct) : access.Order.CustomerId is { } customer ? [customer] : [];
        foreach (var recipientId in recipients)
        {
            await liveNotifier.NotifyAsync(recipientId, orderId, ct);
        }
        await notifier.NotifyAsync(orderId, OrderNotice.ChatMessage, text.Length > 300 ? text[..300] + "…" : text, ct);
        return null;
    }

    private async Task<List<Guid>> StaffIdsAsync(Guid? sellerId, CancellationToken ct)
    {
        if (sellerId is null) return [];
        var ids = await db.SellerEmployees.Where(e => e.SellerId == sellerId && e.UserId != null).Select(e => e.UserId!.Value).ToListAsync(ct);
        var owner = await db.SellerProfiles.Where(s => s.Id == sellerId).Select(s => s.UserId).FirstOrDefaultAsync(ct);
        if (owner.HasValue) ids.Add(owner.Value);
        return ids;
    }

    private sealed record Access(Order Order, string Role, bool CanPost, string SellerName, bool Closed = false);

    private async Task<Access?> ResolveAsync(Guid orderId, CancellationToken ct)
    {
        var me = currentUser.UserId;
        if (me is null) return null;

        var order = await db.Orders.AsNoTracking().Include(o => o.Seller).Where(o => o.Id == orderId).FirstOrDefaultAsync(ct);
        if (order is null) return null;

        var sellerName = order.Seller?.CompanyName ?? "SRIVIDIKA";
        // Chat stays open until the order is delivered (or cancelled), then becomes read-only.
        var closed = order.Status is OrderStatus.Delivered or OrderStatus.Cancelled;
        if (order.CustomerId == me) return new Access(order, "Buyer", !closed, sellerName, closed);

        var isAdmin = currentUser.IsInRole(Roles.Admin);
        if (order.SellerId is null) return isAdmin ? new Access(order, "Seller", !closed, sellerName, closed) : null;
        if (order.Seller?.UserId == me || (order.SellerId is { } sid && await db.SellerEmployees.AnyAsync(e => e.SellerId == sid && e.UserId == me, ct)))
            return new Access(order, "Seller", !closed, sellerName, closed);
        return isAdmin ? new Access(order, "Observer", false, sellerName) : null;
    }

    private static string Answer(string faqId, Order o, string seller)
    {
        var money = o.ShippingFeeConfirmed ? $"Rs. {o.ShippingFee:0.##}" : null;
        return faqId switch
        {
            "status" => $"Your order {o.OrderNumber} is currently {o.Status}. Payment status: {o.PaymentStatus}.",
            "shipping" => o.Status switch
            {
                OrderStatus.Shipped or OrderStatus.Delivered => "Your order has already been shipped.",
                OrderStatus.Cancelled => "This order was cancelled, so it will not be shipped.",
                OrderStatus.Pending => $"{seller} will confirm your order first. Once it is confirmed and paid, it will be prepared and shipped. Handmade items can take a little time, so feel free to ask {seller} for an estimate here.",
                _ => $"Your order is confirmed and being prepared by {seller}. You will see the status change to Shipped as soon as it is on its way.",
            },
            "tracking" => !string.IsNullOrWhiteSpace(o.TrackingNumber)
                ? $"Your tracking number is {o.TrackingNumber}{(string.IsNullOrWhiteSpace(o.ShippingCarrier) ? "" : $" with {o.ShippingCarrier}")}."
                : "Tracking details appear on this page once the order is shipped.",
            "delivery-charge" => money is not null
                ? $"The confirmed delivery charge for this order is {money}."
                : $"Delivery charges vary by location. {seller} will confirm the delivery charge and your final total before payment.",
            "payment" => o.Status == OrderStatus.Pending
                ? "Payment opens after the seller confirms your order and the delivery charge. You will then see the payment QR/UPI details on this page."
                : o.PaymentStatus == PaymentStatus.Paid
                    ? "Your payment is already marked as paid. Thank you!"
                    : "Use the payment QR/UPI details shown on this page, then submit your payment reference so the seller can verify it.",
            "cancel" => o.Status == OrderStatus.Pending
                ? "Yes. While your order is still waiting for confirmation you can cancel it using the Cancel Order button on this page."
                : o.Status == OrderStatus.Cancelled
                    ? "This order is already cancelled."
                    : $"Once an order is confirmed it can no longer be cancelled directly. Please message {seller} here and they will help.",
            "custom" => $"Handmade pieces can often be adjusted. Send your request in a message here and {seller} will reply with what is possible.",
            "problem" => $"Sorry about that! Please describe the issue in a message here and {seller} will get back to you.",
            _ => $"Please send a message and {seller} will reply.",
        };
    }
}