using Domain.Interfaces;

namespace Domain.Entities.User.Interfaces;

/// <summary>
/// Represents a repository for managing user entities.
/// </summary>
public interface IUserRepository : IRepository<UserEntity>
{
}