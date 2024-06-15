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
    /// Get or sets the content of the comment.
    /// </summary>
    public Message Message { get; init; } = default!;

    /// <summary>
    /// Gets or sets the ID of the author of the comment.
    /// </summary>
    public Guid AuthorId { get; init; }

    /// <summary>
    /// Gets or sets the ID of the post that the comment belongs to.
    /// </summary>
    public Guid PostId { get; init; }

    /// <summary>
    /// Gets or sets the post that the comment belongs to.
    /// </summary>
    public virtual PostEntity Post { get; set; } = default!;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommentEntity"/> class.
    /// </summary>

    private CommentEntity(Message message, Guid authorId, PostEntity post)
    {
        Message = message;
        AuthorId = authorId;
        PostId = post.Id;
        Post = post;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="CommentEntity"/> class.
    /// </summary>
    /// <param name="message">The content of the comment.</param>
    /// <param name="authorId">The ID of the author of the comment.</param>
    /// <param name="post">The post that the comment belongs to.</param>
    /// <returns>A new instance of the <see cref="CommentEntity"/> class.</returns>
    public static CommentEntity Create(Message message, Guid authorId, PostEntity post)
    {
        return new CommentEntity(message, authorId, post);
    }
}
