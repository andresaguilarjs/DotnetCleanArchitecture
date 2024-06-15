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
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Contains the date and time when the entity was last updated.
    /// </summary>
    public DateTime LastUpdatedAt { get; private set; }

    public BaseEntity()
    {
        CreatedAt = DateTime.UtcNow;
    }


    /// <summary>
    /// Refreshes the LastUpdatedAt property with the current UTC date and time.
    /// </summary>
    protected void RefreshUpdateAt()
    {
        LastUpdatedAt = DateTime.UtcNow;
    }
}