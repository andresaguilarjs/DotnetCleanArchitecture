namespace Domain.Abstractions;

/// <summary>
/// Represents the base entity class for all entities in the domain.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Contains the unique identifier of the entity.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Contains the date and time when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Contains the date and time when the entity was last updated.
    /// </summary>
    public DateTime LastUpdatedAt { get; set; }
}