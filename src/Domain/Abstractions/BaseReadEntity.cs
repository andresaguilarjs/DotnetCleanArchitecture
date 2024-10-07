namespace Domain.Abstractions;

/// <summary>
/// Represents the base read entity class for all entities in the domain.
/// Read entities are entities that are only read from the database and not modified.
/// </summary>
/// <param name="Id"></param>
public abstract class BaseReadEntity() : BaseEntity;
