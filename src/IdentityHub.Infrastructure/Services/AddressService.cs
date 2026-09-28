using IdentityHub.Application.Common.Interfaces;
using IdentityHub.Application.Common.Models;
using IdentityHub.Domain.Constants;
using IdentityHub.Domain.Entities;
using IdentityHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityHub.Infrastructure.Services;

public sealed class AddressService(AppDbContext db, IActivityLogService activityLog) : IAddressService
{
    public async Task<IReadOnlyList<AddressDto>> GetForUserAsync(Guid userId, CancellationToken ct)
    {
        var addresses = await db.Addresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

        return addresses.Select(ToDto).ToList();
    }

    public async Task<AddressDto?> GetByIdAsync(Guid userId, Guid addressId, CancellationToken ct)
    {
        var address = await db.Addresses.FirstOrDefaultAsync(a => a.UserId == userId && a.Id == addressId, ct);
        return address is null ? null : ToDto(address);
    }

    public async Task<Result<AddressDto>> CreateAsync(Guid userId, AddressInput input, CancellationToken ct)
    {
        var isFirstAddress = !await db.Addresses.AnyAsync(a => a.UserId == userId, ct);

        var address = new Address
        {
            UserId = userId,
            Label = input.Label,
            FullName = input.FullName,
            Phone = input.Phone,
            Line1 = input.Line1,
            Line2 = input.Line2,
            City = input.City,
            State = input.State,
            PostalCode = input.PostalCode,
            Country = input.Country,
            FormattedAddress = input.FormattedAddress,
            Latitude = input.Latitude,
            Longitude = input.Longitude,
            // The very first address a user saves is automatically their default.
            IsDefault = input.IsDefault || isFirstAddress
        };

        if (address.IsDefault)
        {
            await ClearExistingDefaultAsync(userId, ct);
        }

        db.Addresses.Add(address);
        await db.SaveChangesAsync(ct);
        await activityLog.LogAsync(EntityTypes.Address, address.Id.ToString(), "Created", $"Added address \"{address.Label}\".", ct);

        return Result<AddressDto>.Success(ToDto(address));
    }

    public async Task<Result<AddressDto>> UpdateAsync(Guid userId, Guid addressId, AddressInput input, CancellationToken ct)
    {
        var address = await db.Addresses.FirstOrDefaultAsync(a => a.UserId == userId && a.Id == addressId, ct);
        if (address is null)
        {
            return Result<AddressDto>.Failure("Address not found.");
        }

        address.Label = input.Label;
        address.FullName = input.FullName;
        address.Phone = input.Phone;
        address.Line1 = input.Line1;
        address.Line2 = input.Line2;
        address.City = input.City;
        address.State = input.State;
        address.PostalCode = input.PostalCode;
        address.Country = input.Country;
        address.FormattedAddress = input.FormattedAddress;
        address.Latitude = input.Latitude;
        address.Longitude = input.Longitude;
        address.UpdatedAt = DateTimeOffset.UtcNow;

        if (input.IsDefault && !address.IsDefault)
        {
            await ClearExistingDefaultAsync(userId, ct);
            address.IsDefault = true;
        }

        await db.SaveChangesAsync(ct);
        await activityLog.LogAsync(EntityTypes.Address, address.Id.ToString(), "Updated", $"Updated address \"{address.Label}\".", ct);

        return Result<AddressDto>.Success(ToDto(address));
    }

    public async Task<Result> DeleteAsync(Guid userId, Guid addressId, CancellationToken ct)
    {
        var address = await db.Addresses.FirstOrDefaultAsync(a => a.UserId == userId && a.Id == addressId, ct);
        if (address is null)
        {
            return Result.Failure("Address not found.");
        }

        address.IsDeleted = true;
        address.DeletedAt = DateTimeOffset.UtcNow;

        // Promote another address to default so checkout always has one to preselect, if any remain.
        if (address.IsDefault)
        {
            var next = await db.Addresses
                .Where(a => a.UserId == userId && a.Id != addressId)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync(ct);
            if (next is not null)
            {
                next.IsDefault = true;
            }
        }

        await db.SaveChangesAsync(ct);
        await activityLog.LogAsync(EntityTypes.Address, address.Id.ToString(), "Deleted", $"Deleted address \"{address.Label}\".", ct);

        return Result.Success();
    }

    public async Task<Result> SetDefaultAsync(Guid userId, Guid addressId, CancellationToken ct)
    {
        var address = await db.Addresses.FirstOrDefaultAsync(a => a.UserId == userId && a.Id == addressId, ct);
        if (address is null)
        {
            return Result.Failure("Address not found.");
        }

        await ClearExistingDefaultAsync(userId, ct);
        address.IsDefault = true;
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }

    private async Task ClearExistingDefaultAsync(Guid userId, CancellationToken ct)
    {
        var current = await db.Addresses.Where(a => a.UserId == userId && a.IsDefault).ToListAsync(ct);
        foreach (var a in current)
        {
            a.IsDefault = false;
        }
    }

    private static AddressDto ToDto(Address a) => new()
    {
        Id = a.Id,
        Label = a.Label,
        FullName = a.FullName,
        Phone = a.Phone,
        Line1 = a.Line1,
        Line2 = a.Line2,
        City = a.City,
        State = a.State,
        PostalCode = a.PostalCode,
        Country = a.Country,
        FormattedAddress = a.FormattedAddress,
        Latitude = a.Latitude,
        Longitude = a.Longitude,
        IsDefault = a.IsDefault,
        CreatedAt = a.CreatedAt
    };
}
