using Domain.Common;
using Domain.Entities.Comment.ValueObjects;
using Domain.Entities.Post;

namespace Domain.Entities.Comment;

public class CommentEntity : BaseEntity
{
    public Message Message { get; set; } = default!;
    public Guid AuthorId { get; set; }
    public Guid PostId { get; set; }
    public PostEntity Post { get; set; } = default!;
}
