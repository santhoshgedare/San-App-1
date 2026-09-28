namespace IdentityHub.Domain.Entities;

/// <summary>
/// A saved shipping/billing address belonging to a user. Users may have multiple addresses
/// (e.g. Home, Office) and pick one at checkout; one address per user can be marked default.
/// Coordinates (Latitude/Longitude) are captured via the Google Maps picker on the client so
/// deliveries can be located precisely.
/// </summary>
public sealed class Address
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    /// <summary>Short user-facing label, e.g. "Home", "Office".</summary>
    public string Label { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    /// <summary>Formatted address string as resolved by Google Maps Places/Geocoding, for display convenience.</summary>
    public string? FormattedAddress { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public bool IsDefault { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
