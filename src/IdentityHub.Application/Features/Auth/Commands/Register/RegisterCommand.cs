using FluentValidation;
using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using MediatR;

namespace IdentityHub.Application.Features.Auth.Commands.Register;

public sealed record RegisterCommand(string? Email, string Password, string? FirstName, string? LastName, string? PhoneNumber) : IRequest<Result<AuthResultDto>>;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => string.IsNullOrWhiteSpace(x.Email) != string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Provide either an email address or a phone number.");
        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^[+\d][\d\s-]{6,19}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FirstName).MaximumLength(100);
        RuleFor(x => x.LastName).MaximumLength(100);
    }
}

public sealed class RegisterCommandHandler(
    IIdentityService identityService,
    ITokenService tokenService,
    IRefreshTokenService refreshTokenService) : IRequestHandler<RegisterCommand, Result<AuthResultDto>>
{
    public async Task<Result<AuthResultDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var result = await identityService.RegisterAsync(
            request.Email,
            request.Password,
            request.FirstName ?? string.Empty,
            request.LastName ?? string.Empty,
            request.PhoneNumber,
            cancellationToken);
        if (!result.Succeeded)
        {
            return Result<AuthResultDto>.Failure(result.Errors);
        }

        var user = result.Data!;
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
