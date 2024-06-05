using Domain.Common;

namespace Domain.Entities;

public class Comment : BaseEntity
{
    public Message Message { get; set; } = default!;
    public Guid AuthorId { get; set; }
    public Guid PostId { get; set; }
    public Post Post { get; set; } = default!;
}
