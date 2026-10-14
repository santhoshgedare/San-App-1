using System.Security.Cryptography;
using System.Text;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Infrastructure.Email;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace IdentityHub.Infrastructure.Otp;

/// <summary>
/// Issues six-digit codes held (hashed) in memory for a few minutes. Codes are sent straight through
/// SMTP/SMS rather than the email log so they are never persisted. With multiple API instances,
/// swap the cache for a distributed one.
/// </summary>
public sealed class OtpService(
    IMemoryCache cache,
    SmtpEmailSender smtp,
    ISmsSender sms,
    ILogger<OtpService> logger) : IOtpService
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(30);
    private const int MaxAttempts = 5;

    private sealed class Entry
    {
        public required byte[] Salt { get; init; }
        public required byte[] Hash { get; init; }
        public required DateTimeOffset SentAt { get; init; }
        public int Attempts { get; set; }
    }

    public async Task<Result> SendAsync(OtpTarget target, CancellationToken ct)
    {
        var key = CacheKey(target);
        if (cache.TryGetValue(key, out Entry? existing) && existing is not null &&
            DateTimeOffset.UtcNow - existing.SentAt < ResendCooldown)
        {
            return Result.Failure("Please wait a few seconds before requesting another code.");
        }

        var deliverable = target.Channel == OtpChannel.Email ? smtp.IsConfigured : sms.IsConfigured;
        if (!deliverable)
        {
            return Result.Failure(target.Channel == OtpChannel.Email
                ? "Email delivery is not configured."
                : "SMS delivery is not configured.");
        }

        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var salt = RandomNumberGenerator.GetBytes(16);
        var entry = new Entry { Salt = salt, Hash = Hash(code, salt, target), SentAt = DateTimeOffset.UtcNow };
        cache.Set(key, entry, Lifetime);

        try
        {
            var minutes = (int)Lifetime.TotalMinutes;
            if (target.Channel == OtpChannel.Email)
            {
                await smtp.SendAsync(new EmailMessage
                {
                    To = [target.Value],
                    Subject = "Your SRIVIDIKA verification code",
                    HtmlBody = $"<p>Your SRIVIDIKA verification code is <strong style=\"font-size:20px;letter-spacing:3px\">{code}</strong>.</p><p>It expires in {minutes} minutes. If you did not request it, you can ignore this email.</p>",
                    TextBody = $"Your SRIVIDIKA verification code is {code}. It expires in {minutes} minutes.",
                    Category = "Otp"
                }, ct);
            }
            else
            {
                await sms.SendOtpAsync(target.Value, code, minutes, ct);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            cache.Remove(key);
            throw;
        }
        catch (Exception ex)
        {
            cache.Remove(key);
            logger.LogError(ex, "Could not deliver a one-time passcode over {Channel}.", target.Channel);
            return Result.Failure("We couldn't send the code. Please try again shortly.");
        }

        return Result.Success();
    }

    public Result Verify(OtpTarget target, string code)
    {
        const string invalid = "That code is incorrect or has expired.";
        var key = CacheKey(target);
        if (!cache.TryGetValue(key, out Entry? entry) || entry is null) return Result.Failure(invalid);

        lock (entry)
        {
            if (entry.Attempts >= MaxAttempts)
            {
                cache.Remove(key);
                return Result.Failure("Too many incorrect attempts. Request a new code.");
            }
            entry.Attempts++;
        }

        var supplied = Hash((code ?? string.Empty).Trim(), entry.Salt, target);
        if (!CryptographicOperations.FixedTimeEquals(supplied, entry.Hash)) return Result.Failure(invalid);

        cache.Remove(key);
        return Result.Success();
    }

    private static string CacheKey(OtpTarget target) => $"otp:{target.Channel}:{target.Value}";

    private static byte[] Hash(string code, byte[] salt, OtpTarget target)
        => HMACSHA256.HashData(salt, Encoding.UTF8.GetBytes($"{target.Value}:{code}"));
}
