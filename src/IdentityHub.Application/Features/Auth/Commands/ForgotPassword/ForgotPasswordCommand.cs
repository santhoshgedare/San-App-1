using System.Text.Encodings.Web;
using FluentValidation;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IdentityHub.Application.Features.Auth.Commands.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email, string ClientBaseUrl) : IRequest<Result>;

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(request => request.ClientBaseUrl).Must(value =>
            Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp));
    }
}

public sealed class ForgotPasswordCommandHandler(
    IIdentityService identityService,
    IEmailSender emailSender,
    ILogger<ForgotPasswordCommandHandler> logger) : IRequestHandler<ForgotPasswordCommand, Result>
{
    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        if (!emailSender.IsConfigured)
        {
            return Result.Failure("Password recovery email is not configured.");
        }

        var user = await identityService.FindByEmailAsync(request.Email, ct);
        if (user is null || !user.IsActive) return Result.Success();

        var token = await identityService.GeneratePasswordResetTokenAsync(request.Email, ct);
        if (string.IsNullOrWhiteSpace(token)) return Result.Success();

        var resetUrl = $"{request.ClientBaseUrl.TrimEnd('/')}/reset-password?email={Uri.EscapeDataString(request.Email)}&token={Uri.EscapeDataString(token)}";
        var safeName = HtmlEncoder.Default.Encode(user.FirstName);
        var safeUrl = HtmlEncoder.Default.Encode(resetUrl);
        var htmlBody = $"<p>Hello {safeName},</p><p>We received a request to reset your SRIVIDIKA password.</p><p><a href=\"{safeUrl}\">Reset your password</a></p><p>This link can only be used once. If you did not request a reset, you can ignore this email.</p>";
        var textBody = $"Hello {user.FirstName},\n\nReset your SRIVIDIKA password using this one-time link:\n{resetUrl}\n\nIf you did not request a reset, ignore this email.";

        try
        {
            await emailSender.SendAsync(request.Email, "Reset your SRIVIDIKA password", htmlBody, textBody, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not send password reset email for user {UserId}.", user.Id);
        }

        return Result.Success();
    }
}