using Domain.Abstractions;
using Domain.Common;

namespace Domain.Interfaces;

/// <summary>
/// Represents a generic repository interface for querying entities.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public interface IQueryRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the entity.</returns>
    Task<Result<T>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a list of all entities.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of entities.</returns>
    Task<Result<IReadOnlyList<T>>> ListAllAsync(CancellationToken cancellationToken = default);
}