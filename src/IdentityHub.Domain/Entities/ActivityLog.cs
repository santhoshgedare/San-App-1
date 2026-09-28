namespace IdentityHub.Domain.Entities;

/// <summary>
/// Common audit trail entry. Linked to any entity via <see cref="EntityType"/> (a short
/// discriminator such as "User" or "Role") plus <see cref="EntityId"/> (the entity's
/// primary key, stored as a string so this table can reference any key type/shape).
/// </summary>
public sealed class ActivityLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Discriminator for the entity that was acted on, e.g. "User", "Role".</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Primary key of the affected entity, as a string so any key type can be logged.</summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>Short action verb, e.g. "Created", "Updated", "Deleted", "RolesAssigned".</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Free-form human-readable detail about what changed.</summary>
    public string? Details { get; set; }

    public Guid? PerformedByUserId { get; set; }
    public string? PerformedByEmail { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
