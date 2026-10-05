using IdentityHub.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace IdentityHub.Infrastructure.Email;

public sealed class SmtpEmailSender(IOptions<SmtpEmailOptions> options) : IEmailSender
{
    private readonly SmtpEmailOptions settings = options.Value;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(settings.Host) &&
        settings.Port is > 0 and <= 65535 &&
        !string.IsNullOrWhiteSpace(settings.FromAddress) &&
        (string.IsNullOrWhiteSpace(settings.Username) == string.IsNullOrWhiteSpace(settings.Password));

    public async Task SendAsync(string recipient, string subject, string htmlBody, string textBody, CancellationToken ct)
    {
        if (!IsConfigured) throw new InvalidOperationException("SMTP email settings are not configured.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(settings.FromName, settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(recipient));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody, TextBody = textBody }.ToMessageBody();

        using var client = new SmtpClient();
        client.Timeout = Math.Clamp(settings.TimeoutSeconds, 1, 120) * 1000;
        var socketOptions = settings.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
        await client.ConnectAsync(settings.Host, settings.Port, socketOptions, ct);
        if (!string.IsNullOrWhiteSpace(settings.Username))
        {
            await client.AuthenticateAsync(settings.Username, settings.Password, ct);
        }
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }
}