using FluentValidation;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Auth.Commands.Otp;

public sealed record VerifyOtpCommand(string Identifier, string Code, OtpPurpose Purpose, string? FirstName, string? LastName)
    : IRequest<Result<AuthResultDto>>;

public sealed class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator()
    {
        RuleFor(x => x.Identifier).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Code).NotEmpty().Matches("^[0-9]{6}$").WithMessage("Enter the 6-digit code.");
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100).When(x => x.Purpose == OtpPurpose.Register);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100).When(x => x.Purpose == OtpPurpose.Register);
    }
}

public sealed class VerifyOtpCommandHandler(
    IOtpService otpService,
    IIdentityService identityService,
    ITokenService tokenService,
    IRefreshTokenService refreshTokenService) : IRequestHandler<VerifyOtpCommand, Result<AuthResultDto>>
{
    public async Task<Result<AuthResultDto>> Handle(VerifyOtpCommand request, CancellationToken ct)
    {
        var parsed = OtpTarget.Parse(request.Identifier);
        if (!parsed.Succeeded) return Result<AuthResultDto>.Failure(parsed.Errors);
        var target = parsed.Data!;

        var verified = otpService.Verify(target, request.Code);
        if (!verified.Succeeded) return Result<AuthResultDto>.Failure(verified.Errors);

        var identity = await identityService.AuthenticateOtpAsync(
            target,
            request.FirstName ?? string.Empty,
            request.LastName ?? string.Empty,
            allowCreate: request.Purpose == OtpPurpose.Register,
            ct);
        if (!identity.Succeeded) return Result<AuthResultDto>.Failure(identity.Errors);

        var user = identity.Data!;
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
