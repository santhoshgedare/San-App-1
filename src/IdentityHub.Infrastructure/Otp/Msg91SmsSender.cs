using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdentityHub.Infrastructure.Otp;

public sealed class SmsOptions
{
    /// <summary>Development only: write the OTP to the application log instead of sending it.</summary>
    public bool LogOnly { get; set; }

    /// <summary>MSG91 auth key (keep in user-secrets / environment variables).</summary>
    public string AuthKey { get; set; } = string.Empty;

    /// <summary>MSG91 OTP template ID; the template must contain the ##OTP## variable and be DLT-approved.</summary>
    public string OtpTemplateId { get; set; } = string.Empty;
}

public interface ISmsSender
{
    bool IsConfigured { get; }
    Task SendOtpAsync(string phoneNumber, string code, int expiryMinutes, CancellationToken ct);
}

/// <summary>Delivers OTP codes through the MSG91 OTP API, or logs them when <see cref="SmsOptions.LogOnly"/> is set.</summary>
public sealed class Msg91SmsSender(HttpClient http, IOptions<SmsOptions> options, ILogger<Msg91SmsSender> logger) : ISmsSender
{
    private readonly SmsOptions settings = options.Value;

    public bool IsConfigured =>
        settings.LogOnly ||
        (!string.IsNullOrWhiteSpace(settings.AuthKey) && !string.IsNullOrWhiteSpace(settings.OtpTemplateId));

    public async Task SendOtpAsync(string phoneNumber, string code, int expiryMinutes, CancellationToken ct)
    {
        if (!IsConfigured) throw new InvalidOperationException("MSG91 settings are not configured.");

        if (settings.LogOnly)
        {
            logger.LogWarning("SMS (log-only mode) OTP for {Phone}: {Code}", phoneNumber, code);
            return;
        }

        // MSG91 expects the mobile number with country code and no leading '+'.
        var mobile = phoneNumber.TrimStart('+');
        var url = "https://control.msg91.com/api/v5/otp" +
                  $"?template_id={Uri.EscapeDataString(settings.OtpTemplateId)}" +
                  $"&mobile={Uri.EscapeDataString(mobile)}" +
                  $"&otp={Uri.EscapeDataString(code)}" +
                  $"&otp_expiry={expiryMinutes}";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("authkey", settings.AuthKey);
        request.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        using var response = await http.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        response.EnsureSuccessStatusCode();

        // MSG91 can return HTTP 200 with {"type":"error","message":"..."}.
        using var json = JsonDocument.Parse(body);
        if (!json.RootElement.TryGetProperty("type", out var type) ||
            !string.Equals(type.GetString(), "success", StringComparison.OrdinalIgnoreCase))
        {
            var message = json.RootElement.TryGetProperty("message", out var m) ? m.GetString() : body;
            throw new InvalidOperationException($"MSG91 rejected the OTP request: {message}");
        }
    }
}
