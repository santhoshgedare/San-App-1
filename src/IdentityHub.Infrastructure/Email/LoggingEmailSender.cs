using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Domain.Entities;
using IdentityHub.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace IdentityHub.Infrastructure.Email;

/// <summary>Sends through SMTP and records every attempt (sent or failed) in the EmailLogs table.</summary>
public sealed class LoggingEmailSender(
    SmtpEmailSender smtp,
    AppDbContext db,
    ICurrentUserService currentUser,
    ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public bool IsConfigured => smtp.IsConfigured;

    public Task SendAsync(string recipient, string subject, string htmlBody, string textBody, CancellationToken ct)
        => SendAsync(new EmailMessage { To = [recipient], Subject = subject, HtmlBody = htmlBody, TextBody = textBody, Category = "System" }, ct);

    public async Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        var entry = new EmailLog
        {
            ToAddresses = string.Join(", ", message.To),
            CcAddresses = message.Cc.Count > 0 ? string.Join(", ", message.Cc) : null,
            Subject = message.Subject,
            HtmlBody = message.HtmlBody,
            TextBody = message.TextBody,
            Attachments = message.Attachments.Count > 0
                ? string.Join(", ", message.Attachments.Select(a => $"{a.FileName} ({a.Content.Length / 1024 + 1} KB)"))
                : null,
            Category = message.Category,
            RelatedEntityType = message.RelatedEntityType,
            RelatedEntityId = message.RelatedEntityId,
            CreatedByEmail = currentUser.Email
        };

        try
        {
            await smtp.SendAsync(message, ct);
            entry.Status = "Sent";
            entry.SentAt = DateTimeOffset.UtcNow;
        }
        catch (Exception ex)
        {
            entry.Status = "Failed";
            entry.Error = Truncate(ex.Message, 2000);
            logger.LogError(ex, "Email '{Subject}' to {To} failed.", message.Subject, entry.ToAddresses);
            throw;
        }
        finally
        {
            db.EmailLogs.Add(entry);
            await db.SaveChangesAsync(CancellationToken.None);
        }
    }

    private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];
}
