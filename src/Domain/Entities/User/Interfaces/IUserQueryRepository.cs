using Domain.Common;
using Domain.Entities.User.ValueObjects;
using Domain.Interfaces;

namespace Domain.Entities.User.Interfaces;

/// <summary>
/// Represents a repository for querying user entities.
/// </summary>
public interface IUserQueryRepository : IQueryRepository<UserReadEntity>
{
    /// <summary>
    /// Get a user entity by email.
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public Task<Result<UserReadEntity>> GetByEmailAsync(string email);
}