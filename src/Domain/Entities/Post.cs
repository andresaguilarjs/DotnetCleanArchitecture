using Domain.Common;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Post : BaseEntity
{
    public Title Title { get; set; } = default!;
    public PostContent Content { get; set; } = default!;
    public Guid AuthorId { get; set; }
    public User Author { get; set; } = default!;
    public List<Comment> Comments { get; set; } = new();

}
