using IdentityHub.Application.Common.Models;

namespace IdentityHub.Application.Common.Interfaces;

/// <summary>
/// Manages a user's saved addresses (multiple per user, one marked default), used for
/// registration, the profile "My Addresses" page, and quick-select at checkout.
/// </summary>
public interface IAddressService
{
    Task<IReadOnlyList<AddressDto>> GetForUserAsync(Guid userId, CancellationToken ct);
    Task<AddressDto?> GetByIdAsync(Guid userId, Guid addressId, CancellationToken ct);
    Task<Result<AddressDto>> CreateAsync(Guid userId, AddressInput input, CancellationToken ct);
    Task<Result<AddressDto>> UpdateAsync(Guid userId, Guid addressId, AddressInput input, CancellationToken ct);
    Task<Result> DeleteAsync(Guid userId, Guid addressId, CancellationToken ct);
    Task<Result> SetDefaultAsync(Guid userId, Guid addressId, CancellationToken ct);
}
