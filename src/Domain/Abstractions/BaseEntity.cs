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
}