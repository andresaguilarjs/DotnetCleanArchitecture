using Domain.Interfaces;
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
    /// Gets the collection of domain events that have been raised by the entity.
    /// </summary>
    /// <remarks>This collection is typically used to track events for eventual dispatch to external handlers
    /// or systems. The collection is read-only and may be empty if no events have been raised.</remarks>
    public List<IDomainEvent> DomainEvents { get; private set; } = new List<IDomainEvent>();

    /// <summary>
    /// Contains the date and time when the entity was last updated.
    /// </summary>
    public DateTime LastUpdatedAt { get; private set; }

    /// <summary>
    /// Contains a flag indicating whether the entity is deleted.
    /// </summary>
    public bool IsDeleted { get; protected set; }

    public BaseEntity()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent is null)
            return;
        DomainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        DomainEvents.Clear();
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