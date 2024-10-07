using Domain.Common;
using Domain.Interfaces;

namespace Domain.Entities.User.Interfaces;

/// <summary>
/// Represents a repository for managing user entities.
/// </summary>
public interface IUserCommandRepository : ICommandRepository<UserEntity>
{
    public Task<Result<UserEntity>> GetByEmailAsync(string email);
}
