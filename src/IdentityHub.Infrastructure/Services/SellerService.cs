using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Domain.Constants;
using IdentityHub.Domain.Entities;
using IdentityHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdentityHub.Infrastructure.Services;

public sealed class SellerService(
    AppDbContext db,
    IActivityLogService activityLog,
    IIdentityService identityService,
    IAddressService addressService,
    ITokenService tokenService,
    IRefreshTokenService refreshTokenService,
    IEmailSender emailSender,
    ILogger<SellerService> logger) : ISellerService
{
    private static readonly TimeSpan InviteLifetime = TimeSpan.FromDays(7);

    private static string HashToken(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    public async Task<Result<SellerInviteResultDto>> CreateInviteAsync(Guid id, string email, string clientBaseUrl, CancellationToken ct)
    {
        email = email?.Trim() ?? string.Empty;
        if (!MailAddress.TryCreate(email, out _)) return Result<SellerInviteResultDto>.Failure("Enter a valid email address.");

        var seller = await db.SellerProfiles.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (seller is null) return Result<SellerInviteResultDto>.Failure("Seller not found.");
        if (seller.UserId.HasValue) return Result<SellerInviteResultDto>.Failure("This seller already has a registered manager.");

        var existing = await identityService.FindByEmailAsync(email, ct);
        if (existing is not null) return Result<SellerInviteResultDto>.Failure("An account with this email already exists.");

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        var expires = DateTimeOffset.UtcNow.Add(InviteLifetime);
        seller.InviteEmail = email;
        seller.InviteTokenHash = HashToken(token);
        seller.InviteExpiresAt = expires;
        seller.InviteSentAt = DateTimeOffset.UtcNow;
        seller.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        var url = $"{clientBaseUrl.TrimEnd('/')}/seller-register?token={Uri.EscapeDataString(token)}";
        await activityLog.LogAsync(EntityTypes.User, seller.Id.ToString(), "SellerInvited", $"Invitation prepared for '{seller.CompanyName}' to {email}.", ct);
        var body = $"Hello,\n\nYou have been invited to sell on SRIVIDIKA as {seller.CompanyName}.\n\nComplete your seller registration here:\n{url}\n\nThis link expires in 7 days.\n\nWarm regards,\nSRIVIDIKA";
        return Result<SellerInviteResultDto>.Success(new SellerInviteResultDto
        {
            InviteUrl = url,
            ExpiresAt = expires,
            To = email,
            Subject = "You're invited to sell on SRIVIDIKA",
            Body = body
        });
    }

    public async Task<Result> SendInviteEmailAsync(
        Guid id, IReadOnlyList<string> to, IReadOnlyList<string> cc, string subject, string body,
        IReadOnlyList<EmailAttachment> attachments, CancellationToken ct)
    {
        var seller = await db.SellerProfiles.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct);
        if (seller is null) return Result.Failure("Seller not found.");
        if (seller.UserId.HasValue) return Result.Failure("This seller already has a registered manager.");
        if (seller.InviteTokenHash is null || seller.InviteExpiresAt <= DateTimeOffset.UtcNow)
            return Result.Failure("Create the invitation first; the current invite is missing or expired.");

        var toList = to.Select(a => a.Trim()).Where(a => a.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var ccList = cc.Select(a => a.Trim()).Where(a => a.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (toList.Count == 0) return Result.Failure("Add at least one recipient.");
        var bad = toList.Concat(ccList).FirstOrDefault(a => !MailAddress.TryCreate(a, out _));
        if (bad is not null) return Result.Failure($"'{bad}' is not a valid email address.");
        if (string.IsNullOrWhiteSpace(subject)) return Result.Failure("Subject is required.");
        if (string.IsNullOrWhiteSpace(body)) return Result.Failure("Message body is required.");
        if (attachments.Count > 5 || attachments.Sum(a => (long)a.Content.Length) > 10 * 1024 * 1024)
            return Result.Failure("Attach at most 5 files totalling 10 MB.");

        var message = new EmailMessage
        {
            To = toList,
            Cc = ccList,
            Subject = subject.Trim(),
            TextBody = body,
            HtmlBody = ToHtml(body),
            Attachments = attachments,
            Category = "SellerInvite",
            RelatedEntityType = EntityTypes.User,
            RelatedEntityId = seller.Id.ToString()
        };

        try
        {
            await emailSender.SendAsync(message, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Could not send seller invite for {SellerId}.", seller.Id);
            var reason = emailSender.IsConfigured ? ex.Message : "SMTP is not configured";
            return Result.Failure($"Email could not be sent ({reason}). It was recorded as Failed in the email log; you can share the invite link manually.");
        }

        await db.SellerProfiles.Where(s => s.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.InviteSentAt, DateTimeOffset.UtcNow), ct);
        await activityLog.LogAsync(EntityTypes.User, id.ToString(), "SellerInviteEmailed", $"Invitation emailed to {string.Join(", ", toList)}.", ct);
        return Result.Success();
    }

    private static string ToHtml(string text)
    {
        var encoded = WebUtility.HtmlEncode(text);
        encoded = System.Text.RegularExpressions.Regex.Replace(encoded, @"(https?://[^\s<]+)", "<a href=\"$1\">$1</a>");
        encoded = encoded.Replace("\r\n", "\n").Replace("\n", "<br>");
        return $"<div style=\"font-family:Segoe UI,Arial,sans-serif;font-size:15px;line-height:1.6;color:#2b2320\">{encoded}</div>";
    }
    public async Task<SellerInviteInfoDto?> GetInviteAsync(string token, CancellationToken ct)
    {
        var seller = await FindByInviteAsync(token, ct);
        return seller is null
            ? null
            : new SellerInviteInfoDto { Email = seller.InviteEmail!, CompanyName = seller.CompanyName, Tagline = seller.Tagline, LogoUrl = seller.LogoUrl };
    }

    public async Task<Result<AuthResultDto>> AcceptInviteAsync(string token, SellerRegistrationInput input, CancellationToken ct)
    {
        var seller = await FindByInviteAsync(token, ct);
        if (seller is null) return Result<AuthResultDto>.Failure("This invitation is invalid or has expired.");

        if (string.IsNullOrWhiteSpace(input.FirstName) || string.IsNullOrWhiteSpace(input.LastName))
            return Result<AuthResultDto>.Failure("First and last name are required.");
        if (string.IsNullOrWhiteSpace(input.PhoneNumber)) return Result<AuthResultDto>.Failure("Phone number is required.");
        if (string.IsNullOrWhiteSpace(input.Address?.Line1) || string.IsNullOrWhiteSpace(input.Address.City) ||
            string.IsNullOrWhiteSpace(input.Address.State) || string.IsNullOrWhiteSpace(input.Address.PostalCode) ||
            string.IsNullOrWhiteSpace(input.Address.Country))
            return Result<AuthResultDto>.Failure("A complete address is required.");

        var company = input.Company with { UserId = null };
        var validation = await ValidateAsync(company, seller.Id, false, ct);
        if (validation is not null) return Result<AuthResultDto>.Failure(validation);

        var registered = await identityService.RegisterAsync(seller.InviteEmail!, input.Password, input.FirstName.Trim(), input.LastName.Trim(), input.PhoneNumber.Trim(), ct);
        if (!registered.Succeeded) return Result<AuthResultDto>.Failure(registered.Errors);

        var userId = registered.Data!.Id;
        var roles = await identityService.AssignRolesAsync(userId, [Roles.Manager], ct);
        if (!roles.Succeeded) return Result<AuthResultDto>.Failure(roles.Errors);

        await addressService.CreateAsync(userId, input.Address with { IsDefault = true, FullName = $"{input.FirstName} {input.LastName}".Trim(), Phone = input.PhoneNumber.Trim() }, ct);

        Apply(seller, company, false);
        seller.UserId = userId;
        seller.InviteTokenHash = null;
        seller.InviteExpiresAt = null;
        seller.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        await activityLog.LogAsync(EntityTypes.User, seller.Id.ToString(), "SellerRegistered", $"Seller '{seller.CompanyName}' completed registration.", ct);

        var user = (await identityService.FindByIdAsync(userId, ct))!;
        var (accessToken, expiresAt) = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
        await refreshTokenService.StoreAsync(userId, refreshToken, DateTimeOffset.UtcNow.AddDays(7), ct);

        return Result<AuthResultDto>.Success(new AuthResultDto { AccessToken = accessToken, RefreshToken = refreshToken, ExpiresAt = expiresAt, User = user });
    }

    private async Task<SellerProfile?> FindByInviteAsync(string token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        var hash = HashToken(token.Trim());
        var seller = await db.SellerProfiles.FirstOrDefaultAsync(s => s.InviteTokenHash == hash, ct);
        return seller is { UserId: null, InviteEmail: not null } && seller.InviteExpiresAt > DateTimeOffset.UtcNow ? seller : null;
    }

    private const int MaxLogoLength = 700_000;

    public async Task<IReadOnlyList<SellerDto>> GetAllAsync(CancellationToken ct)
    {
        var sellers = await db.SellerProfiles.AsNoTracking().OrderBy(s => s.CompanyName).ToListAsync(ct);
        return await MapAsync(sellers, ct);
    }

    public async Task<SellerDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var seller = await db.SellerProfiles.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct);
        return seller is null ? null : (await MapAsync([seller], ct))[0];
    }

    public async Task<SellerDto?> GetForUserAsync(Guid userId, CancellationToken ct)
    {
        var seller = await db.SellerProfiles.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == userId, ct);
        return seller is null ? null : (await MapAsync([seller], ct))[0];
    }

    public Task<Guid?> GetSellerIdForUserAsync(Guid userId, CancellationToken ct)
        => db.SellerProfiles.Where(s => s.UserId == userId).Select(s => (Guid?)s.Id).FirstOrDefaultAsync(ct);

    public async Task<Result<SellerDto>> CreateAsync(SellerInput input, CancellationToken ct)
    {
        var validation = await ValidateAsync(input, null, true, ct);
        if (validation is not null) return Result<SellerDto>.Failure(validation);

        var seller = new SellerProfile();
        Apply(seller, input, true);
        db.SellerProfiles.Add(seller);
        await db.SaveChangesAsync(ct);

        await activityLog.LogAsync(EntityTypes.User, seller.Id.ToString(), "SellerCreated", $"Seller company '{seller.CompanyName}' created.", ct);
        return Result<SellerDto>.Success((await GetByIdAsync(seller.Id, ct))!);
    }

    public async Task<Result> UpdateAsync(Guid id, SellerInput input, bool allowLinkChange, CancellationToken ct)
    {
        var seller = await db.SellerProfiles.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (seller is null) return Result.Failure("Seller not found.");

        var validation = await ValidateAsync(input, id, allowLinkChange, ct);
        if (validation is not null) return Result.Failure(validation);

        Apply(seller, input, allowLinkChange);
        seller.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        await activityLog.LogAsync(EntityTypes.User, seller.Id.ToString(), "SellerUpdated", $"Seller company '{seller.CompanyName}' updated.", ct);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct)
    {
        var seller = await db.SellerProfiles.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (seller is null) return Result.Failure("Seller not found.");

        var hasOrders = await db.Orders.IgnoreQueryFilters().AnyAsync(o => o.SellerId == id, ct);
        if (hasOrders) return Result.Failure("This seller has orders and cannot be deleted.");

        var hasItems = await db.Items.AnyAsync(i => i.SellerId == id, ct);
        if (hasItems) return Result.Failure("Reassign or delete this seller's items first.");

        db.SellerProfiles.Remove(seller);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<string?> ValidateAsync(SellerInput input, Guid? existingId, bool checkLink, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.CompanyName)) return "Company name is required.";
        if (input.CompanyName.Trim().Length > 200) return "Company name is too long.";
        if (input.LogoUrl is { Length: > MaxLogoLength }) return "Logo image is too large.";

        var name = input.CompanyName.Trim();
        if (await db.SellerProfiles.AnyAsync(s => s.CompanyName == name && s.Id != existingId, ct))
        {
            return "A seller with this company name already exists.";
        }

        if (checkLink && input.UserId.HasValue)
        {
            var userId = input.UserId.Value;
            var isManager = await (from ur in db.UserRoles
                                   join r in db.Roles on ur.RoleId equals r.Id
                                   where ur.UserId == userId && r.Name == Roles.Manager
                                   select ur.UserId).AnyAsync(ct);
            if (!isManager) return "The linked user must have the Manager role.";

            if (await db.SellerProfiles.AnyAsync(s => s.UserId == userId && s.Id != existingId, ct))
            {
                return "That manager is already linked to another seller.";
            }
        }

        return null;
    }

    private static void Apply(SellerProfile s, SellerInput i, bool applyLink)
    {
        static string? Clean(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

        s.CompanyName = i.CompanyName.Trim();
        s.Tagline = Clean(i.Tagline);
        s.Description = Clean(i.Description);
        s.LogoUrl = Clean(i.LogoUrl);
        s.ContactEmail = Clean(i.ContactEmail);
        s.ContactPhone = Clean(i.ContactPhone);
        s.Website = Clean(i.Website);
        s.AddressLine1 = Clean(i.AddressLine1);
        s.City = Clean(i.City);
        s.State = Clean(i.State);
        s.PostalCode = Clean(i.PostalCode);
        s.Country = Clean(i.Country);
        s.UpiId = Clean(i.UpiId);
        s.PayeeName = Clean(i.PayeeName);
        s.QrCodeImageUrl = string.IsNullOrWhiteSpace(i.QrCodeImageUrl) ? null : i.QrCodeImageUrl;
        s.BankDetails = Clean(i.BankDetails);
        if (applyLink) s.UserId = i.UserId;
    }

    private async Task<IReadOnlyList<SellerDto>> MapAsync(IReadOnlyList<SellerProfile> sellers, CancellationToken ct)
    {
        var ids = sellers.Select(s => s.Id).ToList();
        var userIds = sellers.Where(s => s.UserId.HasValue).Select(s => s.UserId!.Value).ToList();

        var counts = await db.Items.Where(i => i.SellerId != null && ids.Contains(i.SellerId.Value))
            .GroupBy(i => i.SellerId!.Value)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);
        var emails = await db.Users.Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Email })
            .ToDictionaryAsync(u => u.Id, u => u.Email, ct);

        return sellers.Select(s => new SellerDto
        {
            Id = s.Id,
            UserId = s.UserId,
            UserEmail = s.UserId.HasValue ? emails.GetValueOrDefault(s.UserId.Value) : null,
            CompanyName = s.CompanyName,
            Tagline = s.Tagline,
            Description = s.Description,
            LogoUrl = s.LogoUrl,
            ContactEmail = s.ContactEmail,
            ContactPhone = s.ContactPhone,
            Website = s.Website,
            AddressLine1 = s.AddressLine1,
            City = s.City,
            State = s.State,
            PostalCode = s.PostalCode,
            Country = s.Country,
            ItemCount = counts.GetValueOrDefault(s.Id),
            UpiId = s.UpiId,
            PayeeName = s.PayeeName,
            QrCodeImageUrl = s.QrCodeImageUrl,
            BankDetails = s.BankDetails,
            InviteEmail = s.InviteEmail,
            InviteStatus = s.UserId.HasValue
                ? "Accepted"
                : s.InviteTokenHash is null ? "None" : s.InviteExpiresAt > DateTimeOffset.UtcNow ? "Pending" : "Expired"
        }).ToList();
    }
}