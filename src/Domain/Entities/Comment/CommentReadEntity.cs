using Domain.Abstractions;

namespace Domain.Entities.Comment;

public class CommentReadEntity : BaseReadEntity
{
    public string Content { get; init; } = default!;

    public CommentReadEntity(Guid id, string content)
    {
        Content = content;
    }
}
