using System.Net;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Domain.Constants;
using IdentityHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IdentityHub.Infrastructure.Services;

public sealed class OrderNotifier(
    AppDbContext db,
    IEmailSender emailSender,
    ICurrentUserService currentUser,
    IConfiguration configuration,
    ILogger<OrderNotifier> logger) : IOrderNotifier
{
    public async Task NotifyAsync(Guid orderId, OrderNotice notice, string? detail = null, CancellationToken ct = default)
    {
        try
        {
            var order = await db.Orders.AsNoTracking().Include(o => o.Seller).FirstOrDefaultAsync(o => o.Id == orderId, ct);
            if (order is null) return;

            var sellerName = order.Seller?.CompanyName ?? "SRIVIDIKA";
            var actorIsBuyer = order.CustomerId.HasValue && order.CustomerId == currentUser.UserId;
            var toBuyer = new List<string>();
            var toSeller = new List<string>();

            switch (notice)
            {
                case OrderNotice.Placed:
                    toBuyer.Add(order.CustomerEmail);
                    toSeller.AddRange(await SellerSideAsync(order, ct));
                    break;
                case OrderNotice.StatusChanged:
                    toBuyer.Add(order.CustomerEmail);
                    if (actorIsBuyer) toSeller.AddRange(await SellerSideAsync(order, ct));
                    break;
                case OrderNotice.DeliveryCharge:
                case OrderNotice.PaymentUpdated:
                case OrderNotice.Refunded:
                    toBuyer.Add(order.CustomerEmail);
                    break;
                case OrderNotice.PaymentSubmitted:
                    toSeller.AddRange(await SellerSideAsync(order, ct));
                    break;
                case OrderNotice.ChatMessage:
                    if (actorIsBuyer) toSeller.AddRange(await SellerSideAsync(order, ct));
                    else toBuyer.Add(order.CustomerEmail);
                    break;
            }

            var link = $"{(configuration["Client:BaseUrl"] ?? "http://localhost:4200").TrimEnd('/')}/orders/{order.Id}";
            var (subject, headline) = Describe(notice, order.OrderNumber, detail);

            await SendAsync(toBuyer, $"{subject} – {sellerName}", headline, detail, order.OrderNumber, link, order.Id, ct);
            await SendAsync(toSeller, subject, headline, detail, order.OrderNumber, link, order.Id, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Order notification {Notice} for {OrderId} failed.", notice, orderId);
        }
    }

    private async Task<List<string>> SellerSideAsync(Domain.Entities.Order order, CancellationToken ct)
    {
        if (order.Seller is not null)
        {
            if (order.Seller.UserId.HasValue)
            {
                var email = await db.Users.Where(u => u.Id == order.Seller.UserId.Value).Select(u => u.Email).FirstOrDefaultAsync(ct);
                if (!string.IsNullOrWhiteSpace(email)) return [email];
            }
            return string.IsNullOrWhiteSpace(order.Seller.ContactEmail) ? [] : [order.Seller.ContactEmail];
        }

        return await (from ur in db.UserRoles
                      join r in db.Roles on ur.RoleId equals r.Id
                      join u in db.Users on ur.UserId equals u.Id
                      where r.Name == Roles.Admin && u.Email != null
                      select u.Email!).ToListAsync(ct);
    }

    private async Task SendAsync(List<string> recipients, string subject, string headline, string? detail, string orderNumber, string link, Guid orderId, CancellationToken ct)
    {
        var me = currentUser.Email;
        var to = recipients
            .Where(e => !string.IsNullOrWhiteSpace(e) && !string.Equals(e, me, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (to.Count == 0) return;

        var safeDetail = string.IsNullOrWhiteSpace(detail) ? "" : $"<p style=\"margin:12px 0;color:#4a3a30\">{WebUtility.HtmlEncode(detail)}</p>";
        var html = $"<div style=\"font-family:Georgia,serif;max-width:560px;margin:auto;padding:24px;background:#fbf6ec;color:#3a2a22\">" +
                   $"<h2 style=\"color:#6b1f2a;margin:0 0 8px\">SRIVIDIKA</h2><h3 style=\"margin:0\">{WebUtility.HtmlEncode(headline)}</h3>" +
                   $"<p style=\"margin:8px 0;color:#7a6a5c\">Order {WebUtility.HtmlEncode(orderNumber)}</p>{safeDetail}" +
                   $"<p><a href=\"{WebUtility.HtmlEncode(link)}\" style=\"background:#6b1f2a;color:#fff;padding:10px 18px;border-radius:8px;text-decoration:none\">View order</a></p></div>";
        var text = $"{headline}\nOrder {orderNumber}\n{detail}\n\nView order: {link}";

        try
        {
            await emailSender.SendAsync(new EmailMessage
            {
                To = to,
                Subject = subject,
                HtmlBody = html,
                TextBody = text,
                Category = "OrderNotification",
                RelatedEntityType = EntityTypes.Order,
                RelatedEntityId = orderId.ToString()
            }, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // The failure is already recorded in the email log by the sender.
            logger.LogWarning(ex, "Order email to {To} was not sent.", string.Join(", ", to));
        }
    }

    private static (string Subject, string Headline) Describe(OrderNotice notice, string number, string? detail) => notice switch
    {
        OrderNotice.Placed => ($"Order {number} placed", "A new order has been placed"),
        OrderNotice.StatusChanged => ($"Order {number} status updated", "Your order status was updated"),
        OrderNotice.DeliveryCharge => ($"Order {number}: delivery charge confirmed", "Delivery charge confirmed – you can now pay"),
        OrderNotice.PaymentSubmitted => ($"Order {number}: payment submitted", "Payment reference submitted – please verify"),
        OrderNotice.PaymentUpdated => ($"Order {number}: payment update", "Your payment status was updated"),
        OrderNotice.Refunded => ($"Order {number}: refund recorded", "A refund has been recorded"),
        OrderNotice.ChatMessage => ($"Order {number}: new message", "You have a new message on this order"),
        _ => ($"Order {number} update", "Order update")
    };
}