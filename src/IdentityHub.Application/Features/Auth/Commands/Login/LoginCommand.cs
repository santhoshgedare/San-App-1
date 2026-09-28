using FluentValidation;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<Result<AuthResultDto>>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginCommandHandler(
    IIdentityService identityService,
    ITokenService tokenService,
    IRefreshTokenService refreshTokenService) : IRequestHandler<LoginCommand, Result<AuthResultDto>>
{
    public async Task<Result<AuthResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var result = await identityService.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);
        if (!result.Succeeded)
        {
            return Result<AuthResultDto>.Failure(result.Errors);
        }

        var user = result.Data!;
        if (!user.IsActive)
        {
            return Result<AuthResultDto>.Failure("This account has been deactivated.");
        }

        await identityService.SetLastLoginAsync(user.Id, cancellationToken);

        var (accessToken, expiresAt) = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
        await refreshTokenService.StoreAsync(user.Id, refreshToken, DateTimeOffset.UtcNow.AddDays(7), cancellationToken);

        return Result<AuthResultDto>.Success(new AuthResultDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = user
        });
    }
}
