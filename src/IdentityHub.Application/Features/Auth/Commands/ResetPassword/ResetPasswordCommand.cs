using FluentValidation;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IdentityHub.Application.Features.Auth.Commands.ResetPassword;

public sealed record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<Result>;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(request => request.Token).NotEmpty().MaximumLength(2048);
        RuleFor(request => request.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(256);
    }
}

public sealed class ResetPasswordCommandHandler(
    IIdentityService identityService,
    IRefreshTokenService refreshTokenService,
    IEmailSender emailSender,
    ILogger<ResetPasswordCommandHandler> logger) : IRequestHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var result = await identityService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword, ct);
        if (!result.Succeeded) return result;

        var user = await identityService.FindByEmailAsync(request.Email, ct);
        if (user is not null)
        {
            await refreshTokenService.RevokeAllForUserAsync(user.Id, ct);
        }

        if (user is not null && emailSender.IsConfigured)
        {
            try
            {
                var safeName = System.Text.Encodings.Web.HtmlEncoder.Default.Encode(user.FirstName);
                await emailSender.SendAsync(
                    user.Email,
                    "Your SRIVIDIKA password was changed",
                    $"<p>Hello {safeName},</p><p>Your SRIVIDIKA password was just changed. If you did not make this change, contact support immediately.</p>",
                    $"Hello {user.FirstName},\n\nYour SRIVIDIKA password was just changed. If you did not make this change, contact support immediately.",
                    ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Could not send password-change notification for user {UserId}.", user.Id);
            }
        }

        return Result.Success();
    }
}