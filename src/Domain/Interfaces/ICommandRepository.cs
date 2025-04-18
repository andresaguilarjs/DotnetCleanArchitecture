using Domain.Abstractions;
using Domain.Common;

namespace Domain.Interfaces;

/// <summary>
/// Represents a generic repository interface for managing entities.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public interface ICommandRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the added entity.</returns>
    Task<Result<T>> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    void Update(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously deletes an entity from the repository using its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the result of the delete operation, indicating success or failure.</returns>
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}