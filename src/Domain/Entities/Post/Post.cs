using Domain.Entities.User;
using Domain.Entities.Post.ValueObjects;
using Domain.Entities.Comment;
using Domain.Abstractions;

namespace Domain.Entities.Post;

public class PostEntity : BaseEntity
{
    public Title Title { get; set; } = default!;
    public PostContent Content { get; set; } = default!;
    public Guid AuthorId { get; set; }
    public UserEntity Author { get; set; } = default!;
    public List<CommentEntity> Comments { get; set; } = new();

    public override void Delete()
    {
        base.IsDeleted = true;
        RefreshUpdateAt();
    }
}
