using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Domain.Constants;
using IdentityHub.Domain.Entities;
using IdentityHub.Domain.Enums;
using IdentityHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityHub.Infrastructure.Services;

public sealed class CustomRequestService(
    AppDbContext db,
    ICurrentUserService currentUser,
    ISellerService sellers,
    IActivityLogService activityLogs,
    IApprovalService approvals,
    IOrderNotifier notifier) : ICustomRequestService
{
    public const string CustomCategoryName = "Custom Orders";
    private const int MaxImages = 5;
    private const int MaxImageLength = 3_000_000;
    private const int MaxMessageLength = 1000;

    private sealed record Actor(Guid? UserId, Guid? SellerId, bool IsAdmin);

    private async Task<Actor> GetActorAsync(CancellationToken ct)
    {
        var me = currentUser.UserId;
        var sellerId = me.HasValue ? await sellers.GetSellerIdForUserAsync(me.Value, ct) : null;
        return new Actor(me, sellerId, currentUser.IsInRole(Roles.Admin));
    }

    public async Task<Result<CustomRequestDetailDto>> CreateAsync(CreateCustomRequestInput input, CancellationToken ct)
    {
        var actor = await GetActorAsync(ct);
        if (actor.UserId is null) return Result<CustomRequestDetailDto>.Failure("Sign in to raise a custom request.");

        var title = input.Title?.Trim() ?? string.Empty;
        var description = input.Description?.Trim() ?? string.Empty;
        if (title.Length is 0 or > 200) return Result<CustomRequestDetailDto>.Failure("Enter a title of up to 200 characters.");
        if (description.Length is 0 or > 4000) return Result<CustomRequestDetailDto>.Failure("Describe your design in up to 4000 characters.");
        if (input.Quantity is < 1 or > 1000) return Result<CustomRequestDetailDto>.Failure("Quantity must be between 1 and 1000.");
        if (input.Budget is < 0) return Result<CustomRequestDetailDto>.Failure("Budget cannot be negative.");
        if (string.IsNullOrWhiteSpace(input.CustomerName) || string.IsNullOrWhiteSpace(input.CustomerEmail) || string.IsNullOrWhiteSpace(input.CustomerPhone))
        {
            return Result<CustomRequestDetailDto>.Failure("Name, email and phone are required.");
        }

        var images = input.Images ?? [];
        if (images.Count > MaxImages) return Result<CustomRequestDetailDto>.Failure($"You can attach up to {MaxImages} photos.");
        foreach (var image in images)
        {
            if (!IsValidImage(image.Url)) return Result<CustomRequestDetailDto>.Failure("One of the photos is invalid or too large.");
        }

        var request = new CustomRequest
        {
            RequestNumber = $"CRQ-{DateTimeOffset.UtcNow:yyyyMMdd}-{RandomNumberGenerator.GetInt32(10000, 99999)}",
            CustomerId = actor.UserId.Value,
            CustomerName = input.CustomerName.Trim(),
            CustomerEmail = input.CustomerEmail.Trim(),
            CustomerPhone = input.CustomerPhone.Trim(),
            Title = title,
            Description = description,
            Quantity = input.Quantity,
            Budget = input.Budget,
            NeededBy = input.NeededBy,
            Images = images.Select((image, index) => new CustomRequestImage
            {
                Url = image.Url,
                FileName = image.FileName?.Trim() is { Length: > 0 } name ? name[..Math.Min(name.Length, 260)] : null,
                SortOrder = index
            }).ToList()
        };

        db.CustomRequests.Add(request);
        await db.SaveChangesAsync(ct);
        await activityLogs.LogAsync(EntityTypes.CustomRequest, request.Id.ToString(), "Created", $"Custom request {request.RequestNumber}: {request.Title}", ct);

        return Result<CustomRequestDetailDto>.Success((await GetAsync(request.Id, ct))!);
    }

    public async Task<PagedResult<CustomRequestSummaryDto>> GetPagedAsync(string? scope, string? status, int page, int pageSize, CancellationToken ct)
    {
        var actor = await GetActorAsync(ct);
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        if (actor.UserId is null) return new PagedResult<CustomRequestSummaryDto> { Items = [], TotalCount = 0, Page = page, PageSize = pageSize };

        var me = actor.UserId.Value;
        var query = db.CustomRequests.AsNoTracking().AsQueryable();
        var canBrowse = actor.SellerId.HasValue || actor.IsAdmin;

        query = (scope?.ToLowerInvariant()) switch
        {
            "open" when canBrowse => query.Where(r => r.CustomerId != me &&
                (r.Status == CustomRequestStatus.Open || r.Status == CustomRequestStatus.Quoted)),
            "joined" when actor.SellerId.HasValue => query.Where(r => r.Threads.Any(t => t.SellerId == actor.SellerId.Value)),
            "all" when actor.IsAdmin => query,
            _ => query.Where(r => r.CustomerId == me)
        };

        if (Enum.TryParse<CustomRequestStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(r => r.Status == parsedStatus);
        }

        var total = await query.CountAsync(ct);
        var sellerId = actor.SellerId;
        var rows = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new
            {
                r.Id,
                r.RequestNumber,
                r.Title,
                r.Status,
                r.Quantity,
                r.Budget,
                r.NeededBy,
                r.CustomerName,
                r.OrderId,
                r.CreatedAt,
                Thumbnail = r.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault(),
                Interested = r.Threads.Count(t => t.Status == CustomRequestThreadStatus.Active || t.Status == CustomRequestThreadStatus.Awarded),
                Quotes = r.Threads.Count(t => t.QuoteUnitPrice != null &&
                    (t.Status == CustomRequestThreadStatus.Active || t.Status == CustomRequestThreadStatus.Awarded)),
                MyThread = sellerId == null ? (Guid?)null : r.Threads.Where(t => t.SellerId == sellerId).Select(t => (Guid?)t.Id).FirstOrDefault()
            })
            .ToListAsync(ct);

        return new PagedResult<CustomRequestSummaryDto>
        {
            Items = rows.Select(r => new CustomRequestSummaryDto
            {
                Id = r.Id,
                RequestNumber = r.RequestNumber,
                Title = r.Title,
                Status = r.Status.ToString(),
                Quantity = r.Quantity,
                Budget = r.Budget,
                NeededBy = r.NeededBy,
                CustomerName = r.CustomerName,
                ThumbnailUrl = r.Thumbnail,
                InterestedCount = r.Interested,
                QuoteCount = r.Quotes,
                MyThreadId = r.MyThread,
                OrderId = r.OrderId,
                CreatedAt = r.CreatedAt
            }).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CustomRequestDetailDto?> GetAsync(Guid id, CancellationToken ct)
    {
        var actor = await GetActorAsync(ct);
        var request = await LoadAsync(id, tracking: false, ct);
        return request is null ? null : BuildDetail(request, actor);
    }

    public async Task<Result<CustomRequestDetailDto>> ExpressInterestAsync(Guid id, string? message, CancellationToken ct)
    {
        var actor = await GetActorAsync(ct);
        if (actor.SellerId is null) return Result<CustomRequestDetailDto>.Failure("Only sellers can show interest in a request.");

        var request = await LoadAsync(id, tracking: true, ct);
        if (request is null || request.CustomerId == actor.UserId) return Result<CustomRequestDetailDto>.Failure("Request not found.");
        if (!IsBiddable(request)) return Result<CustomRequestDetailDto>.Failure("This request is no longer open for quotes.");

        var seller = await db.SellerProfiles.AsNoTracking().FirstAsync(s => s.Id == actor.SellerId.Value, ct);
        if (!seller.CanSell) return Result<CustomRequestDetailDto>.Failure("Your company must be approved and active before you can quote on requests.");
        var text = (message ?? string.Empty).Trim();
        if (text.Length > MaxMessageLength) return Result<CustomRequestDetailDto>.Failure($"Messages can be at most {MaxMessageLength} characters.");

        var thread = request.Threads.FirstOrDefault(t => t.SellerId == actor.SellerId.Value);
        if (thread is null)
        {
            thread = new CustomRequestThread { CustomRequestId = request.Id, SellerId = actor.SellerId.Value };
            request.Threads.Add(thread);
            db.CustomRequestThreads.Add(thread);
            thread.Messages.Add(SystemMessage(thread, seller.CompanyName, $"{seller.CompanyName} is interested in making this."));
        }
        else if (thread.Status == CustomRequestThreadStatus.Withdrawn)
        {
            thread.Status = CustomRequestThreadStatus.Active;
            db.CustomRequestMessages.Add(SystemMessage(thread, seller.CompanyName, $"{seller.CompanyName} is interested again."));
        }

        if (text.Length > 0)
        {
            db.CustomRequestMessages.Add(new CustomRequestMessage
            {
                Thread = thread, SenderUserId = actor.UserId, SenderRole = "Seller", SenderName = seller.CompanyName, Body = text,
                CreatedAt = DateTimeOffset.UtcNow.AddMilliseconds(5)
            });
        }

        await db.SaveChangesAsync(ct);
        return Result<CustomRequestDetailDto>.Success((await GetAsync(id, ct))!);
    }

    public async Task<Result<CustomRequestDetailDto>> SendMessageAsync(Guid id, Guid threadId, string body, CancellationToken ct)
    {
        var text = (body ?? string.Empty).Trim();
        if (text.Length == 0) return Result<CustomRequestDetailDto>.Failure("Please type a message.");
        if (text.Length > MaxMessageLength) return Result<CustomRequestDetailDto>.Failure($"Messages can be at most {MaxMessageLength} characters.");

        var actor = await GetActorAsync(ct);
        var request = await LoadAsync(id, tracking: true, ct);
        var thread = request?.Threads.FirstOrDefault(t => t.Id == threadId);
        if (request is null || thread is null) return Result<CustomRequestDetailDto>.Failure("Conversation not found.");

        var role = RoleInThread(request, thread, actor);
        if (role is null || role == "Observer") return Result<CustomRequestDetailDto>.Failure("You can only read this conversation.");
        if (!IsBiddable(request) || thread.Status != CustomRequestThreadStatus.Active)
        {
            return Result<CustomRequestDetailDto>.Failure("This conversation is closed.");
        }

        var name = role == "Buyer" ? request.CustomerName : thread.Seller!.CompanyName;
        db.CustomRequestMessages.Add(new CustomRequestMessage { ThreadId = thread.Id, SenderUserId = actor.UserId, SenderRole = role, SenderName = name, Body = text });
        await db.SaveChangesAsync(ct);
        return Result<CustomRequestDetailDto>.Success((await GetAsync(id, ct))!);
    }

    public async Task<Result<CustomRequestDetailDto>> SubmitQuoteAsync(Guid id, Guid threadId, CustomQuoteInput quote, CancellationToken ct)
    {
        if (quote.UnitPrice <= 0) return Result<CustomRequestDetailDto>.Failure("Enter a price greater than zero.");
        if (quote.ShippingFee is < 0) return Result<CustomRequestDetailDto>.Failure("Delivery charge cannot be negative.");
        if (quote.LeadTimeDays is < 1 or > 365) return Result<CustomRequestDetailDto>.Failure("Lead time must be between 1 and 365 days.");
        if (quote.Note?.Length > 500) return Result<CustomRequestDetailDto>.Failure("The quote note can be at most 500 characters.");

        var actor = await GetActorAsync(ct);
        var request = await LoadAsync(id, tracking: true, ct);
        var thread = request?.Threads.FirstOrDefault(t => t.Id == threadId);
        if (request is null || thread is null || thread.SellerId != actor.SellerId) return Result<CustomRequestDetailDto>.Failure("Conversation not found.");
        if (actor.SellerId is { } quotingSellerId && !await db.SellerProfiles.AnyAsync(s => s.Id == quotingSellerId && s.IsActive && s.Status == SellerStatus.Approved, ct))
        {
            return Result<CustomRequestDetailDto>.Failure("Your company must be approved and active before you can quote on requests.");
        }
        if (!IsBiddable(request) || thread.Status != CustomRequestThreadStatus.Active)
        {
            return Result<CustomRequestDetailDto>.Failure("This request is no longer open for quotes.");
        }

        thread.QuoteUnitPrice = quote.UnitPrice;
        thread.QuoteShippingFee = quote.ShippingFee;
        thread.QuoteLeadTimeDays = quote.LeadTimeDays;
        thread.QuoteNote = string.IsNullOrWhiteSpace(quote.Note) ? null : quote.Note.Trim();
        thread.QuotedAt = DateTimeOffset.UtcNow;
        thread.QuoteRevision++;
        request.Status = CustomRequestStatus.Quoted;
        request.UpdatedAt = DateTimeOffset.UtcNow;

        var total = quote.UnitPrice * request.Quantity + (quote.ShippingFee ?? 0);
        var summary = $"{(thread.QuoteRevision > 1 ? "Revised quote" : "Quote")}: {Money(quote.UnitPrice)} each × {request.Quantity} = {Money(total)}" +
            (quote.ShippingFee.HasValue ? $" including {Money(quote.ShippingFee.Value)} delivery" : ", delivery to be confirmed") +
            (quote.LeadTimeDays.HasValue ? $". Ready in about {quote.LeadTimeDays} days." : ".");
        db.CustomRequestMessages.Add(SystemMessage(thread, thread.Seller!.CompanyName, summary));

        await db.SaveChangesAsync(ct);
        return Result<CustomRequestDetailDto>.Success((await GetAsync(id, ct))!);
    }

    public async Task<Result<CustomRequestDetailDto>> WithdrawAsync(Guid id, Guid threadId, CancellationToken ct)
    {
        var actor = await GetActorAsync(ct);
        var request = await LoadAsync(id, tracking: true, ct);
        var thread = request?.Threads.FirstOrDefault(t => t.Id == threadId);
        if (request is null || thread is null || thread.SellerId != actor.SellerId) return Result<CustomRequestDetailDto>.Failure("Conversation not found.");
        if (thread.Status != CustomRequestThreadStatus.Active) return Result<CustomRequestDetailDto>.Failure("This conversation is already closed.");

        thread.Status = CustomRequestThreadStatus.Withdrawn;
        db.CustomRequestMessages.Add(SystemMessage(thread, thread.Seller!.CompanyName, $"{thread.Seller.CompanyName} withdrew from this request."));
        if (request.Status == CustomRequestStatus.Quoted && !request.Threads.Any(t => t.Status == CustomRequestThreadStatus.Active && t.QuoteUnitPrice != null))
        {
            request.Status = CustomRequestStatus.Open;
        }

        await db.SaveChangesAsync(ct);
        return Result<CustomRequestDetailDto>.Success((await GetAsync(id, ct))!);
    }

    public async Task<Result<CustomRequestDetailDto>> CancelAsync(Guid id, CancellationToken ct)
    {
        var actor = await GetActorAsync(ct);
        var request = await LoadAsync(id, tracking: true, ct);
        if (request is null || request.CustomerId != actor.UserId) return Result<CustomRequestDetailDto>.Failure("Request not found.");
        if (!IsBiddable(request)) return Result<CustomRequestDetailDto>.Failure("This request can no longer be cancelled.");

        request.Status = CustomRequestStatus.Cancelled;
        request.UpdatedAt = DateTimeOffset.UtcNow;
        foreach (var thread in request.Threads.Where(t => t.Status == CustomRequestThreadStatus.Active))
        {
            thread.Status = CustomRequestThreadStatus.Closed;
        }

        await db.SaveChangesAsync(ct);
        await activityLogs.LogAsync(EntityTypes.CustomRequest, request.Id.ToString(), "Cancelled", $"Custom request {request.RequestNumber} cancelled by the customer.", ct);
        return Result<CustomRequestDetailDto>.Success((await GetAsync(id, ct))!);
    }

    public async Task<Result<Guid>> AwardAsync(Guid id, Guid threadId, AwardCustomRequestInput input, CancellationToken ct)
    {
        var address = input.ShippingAddress?.Trim() ?? string.Empty;
        if (address.Length == 0) return Result<Guid>.Failure("Enter a shipping address.");

        var actor = await GetActorAsync(ct);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        var request = await LoadAsync(id, tracking: true, ct);
        if (request is null || request.CustomerId != actor.UserId) return Result<Guid>.Failure("Request not found.");
        if (!IsBiddable(request)) return Result<Guid>.Failure("This request has already been awarded or closed.");

        var thread = request.Threads.FirstOrDefault(t => t.Id == threadId);
        if (thread is null || thread.Status != CustomRequestThreadStatus.Active || thread.QuoteUnitPrice is null)
        {
            return Result<Guid>.Failure("Choose a seller who has sent a quote.");
        }

        var seller = thread.Seller!;
        if (seller.UserId == request.CustomerId
            || await db.SellerEmployees.AnyAsync(e => e.SellerId == seller.Id && e.UserId == request.CustomerId, ct))
            return Result<Guid>.Failure("You cannot order from your own company.");

        var category = await db.Categories.FirstOrDefaultAsync(c => c.Name == CustomCategoryName, ct);
        if (category is null)
        {
            category = new Category { Name = CustomCategoryName, Description = "Items created from accepted custom requests.", IsActive = false };
            db.Categories.Add(category);
        }

        var unitPrice = thread.QuoteUnitPrice.Value;
        var shippingFee = thread.QuoteShippingFee ?? 0m;
        var subtotal = unitPrice * request.Quantity;
        var images = request.Images.OrderBy(i => i.SortOrder).ToList();
        var itemCode = $"CUS-{request.RequestNumber[4..]}";

        var item = new Item
        {
            Code = itemCode,
            Name = request.Title.Length > 200 ? request.Title[..200] : request.Title,
            Description = request.Description,
            Category = category,
            Price = unitPrice,
            CostPrice = 0,
            StockQuantity = 0,
            IsActive = false,
            SellerId = seller.Id,
            Images = images.Select((image, index) => new ItemImage
            {
                Url = image.Url,
                FileName = image.FileName,
                IsPrimary = index == 0,
                SortOrder = index + 1
            }).ToList()
        };
        db.Items.Add(item);

        var notes = $"Custom request {request.RequestNumber}." + (string.IsNullOrWhiteSpace(input.OrderNotes) ? string.Empty : $" {input.OrderNotes.Trim()}");
        var order = new Order
        {
            OrderNumber = $"ORD-{DateTimeOffset.UtcNow:yyyyMMdd}-{RandomNumberGenerator.GetInt32(10000, 99999)}",
            SellerId = seller.Id,
            Seller = seller,
            CustomerId = request.CustomerId,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            ShippingAddress = address,
            OrderNotes = notes.Length > 1000 ? notes[..1000] : notes,
            PaymentMethod = PaymentMethod.UpiQr,
            PaymentStatus = PaymentStatus.Pending,
            Status = OrderStatus.Pending,
            SubtotalAmount = subtotal,
            ShippingFee = shippingFee,
            ShippingFeeConfirmed = thread.QuoteShippingFee.HasValue,
            ShippingFeeNote = thread.QuoteShippingFee.HasValue ? "As per the accepted quote" : null,
            ChargesAcceptedAt = thread.QuoteShippingFee.HasValue ? DateTimeOffset.UtcNow : null,
            TotalAmount = subtotal + shippingFee,
            CustomRequestId = request.Id,
            Items =
            [
                new OrderItem
                {
                    Item = item,
                    ItemCode = item.Code,
                    ItemName = item.Name,
                    ImageUrl = images.FirstOrDefault()?.Url,
                    UnitPrice = unitPrice,
                    UnitCostPrice = 0,
                    Quantity = request.Quantity,
                    TotalPrice = subtotal
                }
            ],
            ProgressUpdates =
            [
                new OrderProgressUpdate { Stage = CustomOrderStage.Accepted, Note = $"Quote accepted by the customer ({request.RequestNumber})." }
            ]
        };
        db.Orders.Add(order);

        request.Status = CustomRequestStatus.Awarded;
        request.AwardedThreadId = thread.Id;
        request.OrderId = order.Id;
        request.UpdatedAt = DateTimeOffset.UtcNow;
        thread.Status = CustomRequestThreadStatus.Awarded;
        db.CustomRequestMessages.Add(SystemMessage(thread, request.CustomerName, $"{request.CustomerName} accepted this quote. Order {order.OrderNumber} was created."));
        foreach (var other in request.Threads.Where(t => t.Id != thread.Id && t.Status == CustomRequestThreadStatus.Active))
        {
            other.Status = CustomRequestThreadStatus.Closed;
        }

        await db.SaveChangesAsync(ct);
        await activityLogs.LogAsync(EntityTypes.CustomRequest, request.Id.ToString(), "Awarded",
            $"Custom request {request.RequestNumber} awarded to {seller.CompanyName}; order {order.OrderNumber} created.", ct);
        await activityLogs.LogAsync(EntityTypes.Order, order.Id.ToString(), "Created",
            $"Order {order.OrderNumber} created from custom request {request.RequestNumber} for {Money(order.TotalAmount)}", ct);
        await transaction.CommitAsync(ct);

        await approvals.RequestAsync(
            EntityTypes.Order,
            order.Id.ToString(),
            $"Order Approval: {order.OrderNumber}",
            $"Customer: {order.CustomerName}, Custom request: {request.RequestNumber}, Total: {Money(order.TotalAmount)}",
            ct);
        await notifier.NotifyAsync(order.Id, OrderNotice.Placed, $"Custom order from request {request.RequestNumber}. Total {Money(order.TotalAmount)}.", ct);

        return Result<Guid>.Success(order.Id);
    }

    public async Task<Result> AddProgressAsync(Guid orderId, string stage, string? note, string? imageUrl, CancellationToken ct)
    {
        if (!Enum.TryParse<CustomOrderStage>(stage, true, out var parsedStage) || !Enum.IsDefined(parsedStage))
        {
            return Result.Failure("Choose a valid progress stage.");
        }

        var trimmedNote = note?.Trim();
        if (trimmedNote?.Length > 1000) return Result.Failure("The note can be at most 1000 characters.");
        if (!string.IsNullOrWhiteSpace(imageUrl) && !IsValidImage(imageUrl)) return Result.Failure("The photo is invalid or too large.");

        var actor = await GetActorAsync(ct);
        var order = await db.Orders.Include(o => o.ProgressUpdates).FirstOrDefaultAsync(o => o.Id == orderId && o.CustomRequestId != null, ct);
        if (order is null || order.SellerId is null || order.SellerId != actor.SellerId) return Result.Failure("Custom order not found.");
        if (order.Status == OrderStatus.Cancelled) return Result.Failure("This order was cancelled.");
        if (order.Status == OrderStatus.Pending) return Result.Failure("Confirm the order before posting production progress.");

        var latest = order.ProgressUpdates.Count == 0 ? (CustomOrderStage?)null : order.ProgressUpdates.Max(u => u.Stage);
        if (latest.HasValue && parsedStage < latest.Value) return Result.Failure("Progress cannot move backwards.");

        db.OrderProgressUpdates.Add(new OrderProgressUpdate
        {
            OrderId = order.Id,
            Stage = parsedStage,
            Note = string.IsNullOrWhiteSpace(trimmedNote) ? null : trimmedNote,
            ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl
        });
        await db.SaveChangesAsync(ct);
        await activityLogs.LogAsync(EntityTypes.Order, order.Id.ToString(), "Progress", $"Custom order progress: {parsedStage}", ct);
        await notifier.NotifyAsync(order.Id, OrderNotice.ProgressUpdated, $"Stage: {StageLabel(parsedStage)}." + (trimmedNote is { Length: > 0 } ? $" {trimmedNote}" : string.Empty), ct);
        return Result.Success();
    }

    private Task<CustomRequest?> LoadAsync(Guid id, bool tracking, CancellationToken ct)
    {
        var query = db.CustomRequests
            .Include(r => r.Images)
            .Include(r => r.Threads).ThenInclude(t => t.Seller)
            .Include(r => r.Threads).ThenInclude(t => t.Messages)
            .AsSplitQuery();
        if (!tracking) query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    private static bool IsBiddable(CustomRequest request) =>
        request.Status is CustomRequestStatus.Open or CustomRequestStatus.Quoted;

    private static string? RoleInThread(CustomRequest request, CustomRequestThread thread, Actor actor)
    {
        if (actor.UserId is null) return null;
        if (request.CustomerId == actor.UserId) return "Buyer";
        if (actor.SellerId == thread.SellerId) return "Seller";
        return actor.IsAdmin ? "Observer" : null;
    }

    private CustomRequestDetailDto? BuildDetail(CustomRequest request, Actor actor)
    {
        if (actor.UserId is null) return null;

        var isCustomer = request.CustomerId == actor.UserId;
        var isSeller = !isCustomer && actor.SellerId.HasValue;
        var myThread = isSeller ? request.Threads.FirstOrDefault(t => t.SellerId == actor.SellerId) : null;
        if (!isCustomer && !actor.IsAdmin && !(isSeller && (IsBiddable(request) || myThread is not null))) return null;

        var visible = isCustomer || (actor.IsAdmin && !isSeller)
            ? request.Threads.OrderBy(t => t.CreatedAt).ToList()
            : (myThread is null ? [] : [myThread]);
        var role = isCustomer ? "Customer" : isSeller ? "Seller" : "Observer";

        return new CustomRequestDetailDto
        {
            Id = request.Id,
            RequestNumber = request.RequestNumber,
            Title = request.Title,
            Description = request.Description,
            Status = request.Status.ToString(),
            Quantity = request.Quantity,
            Budget = request.Budget,
            NeededBy = request.NeededBy,
            CustomerName = request.CustomerName,
            CreatedAt = request.CreatedAt,
            OrderId = isCustomer || myThread?.Status == CustomRequestThreadStatus.Awarded || actor.IsAdmin ? request.OrderId : null,
            AwardedThreadId = request.AwardedThreadId,
            MyRole = role,
            CanExpressInterest = isSeller && IsBiddable(request) && (myThread is null || myThread.Status == CustomRequestThreadStatus.Withdrawn),
            Images = request.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).ToList(),
            Threads = visible.Select(t => new CustomRequestThreadDto
            {
                Id = t.Id,
                SellerId = t.SellerId,
                SellerName = t.Seller?.CompanyName ?? string.Empty,
                Status = t.Status.ToString(),
                QuoteUnitPrice = t.QuoteUnitPrice,
                QuoteShippingFee = t.QuoteShippingFee,
                QuoteLeadTimeDays = t.QuoteLeadTimeDays,
                QuoteNote = t.QuoteNote,
                QuotedAt = t.QuotedAt,
                QuoteRevision = t.QuoteRevision,
                QuoteTotal = t.QuoteUnitPrice.HasValue ? t.QuoteUnitPrice.Value * request.Quantity + (t.QuoteShippingFee ?? 0) : null,
                IsMine = t.SellerId == actor.SellerId,
                CreatedAt = t.CreatedAt,
                Messages = t.Messages.OrderBy(m => m.CreatedAt).Take(300).Select(m => new CustomRequestMessageDto
                {
                    Id = m.Id,
                    SenderRole = m.SenderRole,
                    SenderName = m.SenderName,
                    Body = m.Body,
                    CreatedAt = m.CreatedAt,
                    IsMine = m.SenderUserId != null && m.SenderUserId == actor.UserId
                }).ToList()
            }).ToList()
        };
    }

    private static CustomRequestMessage SystemMessage(CustomRequestThread thread, string senderName, string body) => new()
    {
        Thread = thread,
        SenderRole = "System",
        SenderName = senderName,
        Body = body.Length > MaxMessageLength ? body[..MaxMessageLength] : body
    };

    private static bool IsValidImage(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || url.Length > MaxImageLength) return false;
        return url.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase)
            || Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
    }

    private static string Money(decimal amount) => amount.ToString("C", CultureInfo.GetCultureInfo("en-IN"));

    private static string StageLabel(CustomOrderStage stage) => stage switch
    {
        CustomOrderStage.DesignFinalized => "Design finalized",
        CustomOrderStage.InProduction => "In production",
        CustomOrderStage.QualityCheck => "Quality check",
        CustomOrderStage.ReadyToShip => "Ready to ship",
        _ => "Accepted"
    };
}
