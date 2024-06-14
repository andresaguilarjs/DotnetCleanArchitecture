using Domain.Abstractions;
using Domain.Entities.Comment.ValueObjects;
using Domain.Entities.Post;

namespace Domain.Entities.Comment;

/// <summary>
/// Represents a comment entity.
/// </summary>
public class CommentEntity : BaseEntity
{
    /// <summary>
    /// Gets or sets the message of the comment.
    /// </summary>
    public Message Message { get; set; } = default!;

    /// <summary>
    /// Gets or sets the ID of the author of the comment.
    /// </summary>
    public Guid AuthorId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the post that the comment belongs to.
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// Gets or sets the post that the comment belongs to.
    /// </summary>
    public virtual PostEntity Post { get; set; } = default!;
}
