using FluentValidation;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Auth.Commands.ExternalLogin;

public sealed record ExternalLoginCommand(
    string Provider,
    string ProviderKey,
    string Email,
    string FirstName,
    string LastName,
    bool EmailVerified) : IRequest<Result<AuthResultDto>>;

public sealed class ExternalLoginCommandValidator : AbstractValidator<ExternalLoginCommand>
{
    public ExternalLoginCommandValidator()
    {
        RuleFor(request => request.Provider).Must(provider => provider is "Google" or "Facebook");
        RuleFor(request => request.ProviderKey).NotEmpty().MaximumLength(256);
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(request => request.FirstName).MaximumLength(100);
        RuleFor(request => request.LastName).MaximumLength(100);
    }
}

public sealed class ExternalLoginCommandHandler(
    IIdentityService identityService,
    ITokenService tokenService,
    IRefreshTokenService refreshTokenService) : IRequestHandler<ExternalLoginCommand, Result<AuthResultDto>>
{
    public async Task<Result<AuthResultDto>> Handle(ExternalLoginCommand request, CancellationToken ct)
    {
        var identityResult = await identityService.AuthenticateExternalAsync(
            request.Provider,
            request.ProviderKey,
            request.Email,
            request.FirstName,
            request.LastName,
            request.EmailVerified,
            ct);
        if (!identityResult.Succeeded) return Result<AuthResultDto>.Failure(identityResult.Errors);

        var user = identityResult.Data!;
        await identityService.SetLastLoginAsync(user.Id, ct);

        var (accessToken, expiresAt) = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
        await refreshTokenService.StoreAsync(user.Id, refreshToken, DateTimeOffset.UtcNow.AddDays(7), ct);

        return Result<AuthResultDto>.Success(new AuthResultDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = user
        });
    }
}