using Domain.Interfaces;

namespace Domain.Entities.Post.Interfaces;

/// <summary>
/// Represents a repository for managing post entities.
/// </summary>
public interface IPostRepository : IQueryRepository<PostEntity>
{
}
