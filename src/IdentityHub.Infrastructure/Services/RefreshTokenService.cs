using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Domain.Entities;
using IdentityHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityHub.Infrastructure.Services;

public sealed class RefreshTokenService(AppDbContext db) : IRefreshTokenService
{
    public async Task StoreAsync(Guid userId, string token, DateTimeOffset expiresAt, CancellationToken ct)
    {
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt
        });
        await db.SaveChangesAsync(ct);
    }

    public async Task<Guid?> ValidateAndConsumeAsync(string token, CancellationToken ct)
    {
        var entity = await db.RefreshTokens.SingleOrDefaultAsync(rt => rt.Token == token, ct);
        if (entity is null || !entity.IsActive)
        {
            return null;
        }

        entity.RevokedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return entity.UserId;
    }

    public async Task RevokeAsync(string token, CancellationToken ct)
    {
        var entity = await db.RefreshTokens.SingleOrDefaultAsync(rt => rt.Token == token, ct);
        if (entity is not null && entity.IsActive)
        {
            entity.RevokedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct)
    {
        var tokens = await db.RefreshTokens.Where(rt => rt.UserId == userId && rt.RevokedAt == null).ToListAsync(ct);
        foreach (var token in tokens)
        {
            token.RevokedAt = DateTimeOffset.UtcNow;
        }
        await db.SaveChangesAsync(ct);
    }
}
