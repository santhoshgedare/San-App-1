using IdentityHub.Application.Common.Models;

namespace IdentityHub.Application.Common.Interfaces;

public enum OtpChannel
{
    Email,
    Phone
}

/// <summary>A validated, canonical email address or E.164-style phone number.</summary>
public sealed record OtpTarget(OtpChannel Channel, string Value)
{
    public static Result<OtpTarget> Parse(string? identifier)
    {
        var raw = identifier?.Trim() ?? string.Empty;
        if (raw.Length == 0) return Result<OtpTarget>.Failure("Enter your email address or phone number.");

        if (raw.Contains('@'))
        {
            var email = raw.ToLowerInvariant();
            var at = email.IndexOf('@');
            var valid = email.Length <= 256 && at > 0 && at == email.LastIndexOf('@') &&
                        email.IndexOf('.', at) > at + 1 && !email.EndsWith('.') && !email.Any(char.IsWhiteSpace);
            return valid
                ? Result<OtpTarget>.Success(new OtpTarget(OtpChannel.Email, email))
                : Result<OtpTarget>.Failure("Enter a valid email address.");
        }

        var hasPlus = raw.StartsWith('+');
        var digits = new string(raw.Where(char.IsDigit).ToArray());
        var allowed = raw.All(c => char.IsDigit(c) || c is '+' or ' ' or '-' or '(' or ')');
        if (!allowed || digits.Length < 8 || digits.Length > 15)
        {
            return Result<OtpTarget>.Failure("Enter a valid phone number with country code.");
        }

        // A bare 10-digit number is treated as an Indian mobile number.
        var phone = hasPlus ? $"+{digits}" : digits.Length == 10 ? $"+91{digits}" : $"+{digits}";
        return Result<OtpTarget>.Success(new OtpTarget(OtpChannel.Phone, phone));
    }
}

/// <summary>Issues and verifies short-lived one-time passcodes delivered by email or SMS.</summary>
public interface IOtpService
{
    Task<Result> SendAsync(OtpTarget target, CancellationToken ct);
    Result Verify(OtpTarget target, string code);
}
