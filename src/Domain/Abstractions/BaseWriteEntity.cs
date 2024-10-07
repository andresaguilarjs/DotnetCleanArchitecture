﻿namespace Domain.Abstractions;

/// <summary>
/// Represents the base entity class for all entities in the domain.
/// </summary>
public abstract class BaseWriteEntity : BaseEntity
{
    /// <summary>
    /// Contains the date and time when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Contains the date and time when the entity was last updated.
    /// </summary>
    public DateTime LastUpdatedAt { get; private set; }

    /// <summary>
    /// Contains a flag indicating whether the entity is deleted.
    /// </summary>
    public bool IsDeleted { get; protected set; }

    public BaseWriteEntity()
    {
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the entity as deleted.
    /// </summary>
    public abstract void Delete();

    /// <summary>
    /// Refreshes the LastUpdatedAt property with the current UTC date and time.
    /// </summary>
    protected void RefreshUpdateAt()
    {
        LastUpdatedAt = DateTime.UtcNow;
    }
}