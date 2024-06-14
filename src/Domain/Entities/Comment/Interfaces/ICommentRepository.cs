using Domain.Interfaces;

namespace Domain.Entities.Comment.Interfaces;

/// <summary>
/// Represents a repository for managing comments.
/// </summary>
public interface ICommentRepository : IRepository<CommentEntity>
{
}
