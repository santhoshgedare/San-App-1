using FluentValidation;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<Result<AuthResultDto>>;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.AccessToken).NotEmpty();
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public sealed class RefreshTokenCommandHandler(
    ITokenService tokenService,
    IRefreshTokenService refreshTokenService,
    IIdentityService identityService) : IRequestHandler<RefreshTokenCommand, Result<AuthResultDto>>
{
    public async Task<Result<AuthResultDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal is null)
        {
            return Result<AuthResultDto>.Failure("Invalid access token.");
        }

        var userId = await refreshTokenService.ValidateAndConsumeAsync(request.RefreshToken, cancellationToken);
        if (userId is null)
        {
            return Result<AuthResultDto>.Failure("Invalid or expired refresh token.");
        }

        var user = await identityService.FindByIdAsync(userId.Value, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return Result<AuthResultDto>.Failure("Account is no longer available.");
        }

        var (accessToken, expiresAt) = tokenService.GenerateAccessToken(user);
        var newRefreshToken = tokenService.GenerateRefreshToken();
        await refreshTokenService.StoreAsync(user.Id, newRefreshToken, DateTimeOffset.UtcNow.AddDays(7), cancellationToken);

        return Result<AuthResultDto>.Success(new AuthResultDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = expiresAt,
            User = user
        });
    }
}
