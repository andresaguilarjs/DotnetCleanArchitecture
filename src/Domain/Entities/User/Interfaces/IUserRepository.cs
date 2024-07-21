using Domain.Common;
using Domain.Entities.User.ValueObjects;
using Domain.Interfaces;

namespace Domain.Entities.User.Interfaces;

/// <summary>
/// Represents a repository for managing user entities.
/// </summary>
public interface IUserRepository : IRepository<UserEntity>
{
    /// <summary>
    /// Get a user entity by email.
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public Task<Result<UserEntity>> GetByEmailAsync(Email email);
}