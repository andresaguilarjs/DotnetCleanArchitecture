using Domain.Abstractions;

namespace Domain.Entities.Post;

public class PostReadEntity : BaseReadEntity
{
    public string Title { get; init; } = default!;
    public string Content { get; init; } = default!;

    public PostReadEntity(string title, string content)
    {
        Title = title;
        Content = content;
    }
}
