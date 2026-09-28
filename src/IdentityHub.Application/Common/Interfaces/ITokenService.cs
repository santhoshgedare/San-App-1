using System.Security.Claims;
using IdentityHub.Application.Common.Models;

namespace IdentityHub.Application.Common.Interfaces;

/// <summary>
/// Issues and validates JWT access tokens.
/// </summary>
public interface ITokenService
{
    (string Token, DateTimeOffset ExpiresAt) GenerateAccessToken(UserDto user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

/// <summary>
/// Persists and rotates refresh tokens.
/// </summary>
public interface IRefreshTokenService
{
    Task StoreAsync(Guid userId, string token, DateTimeOffset expiresAt, CancellationToken ct);
    Task<Guid?> ValidateAndConsumeAsync(string token, CancellationToken ct);
    Task RevokeAsync(string token, CancellationToken ct);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct);
}
