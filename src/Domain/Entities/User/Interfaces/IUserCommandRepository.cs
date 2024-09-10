using Domain.Interfaces;

namespace Domain.Entities.User.Interfaces;

/// <summary>
/// Represents a repository for managing user entities.
/// </summary>
public interface IUserCommandRepository : ICommandRepository<UserEntity>
{
}
