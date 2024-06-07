using Domain.Entities.Comment;
using Domain.Entities.Comment.Interfaces;
using Infrastructure.Database.Common;

namespace Infrastructure.Database.Repositories;

public class CommentRepository : Repository<CommentEntity>, ICommentRepository
{
    public CommentRepository(ApplicationDbContext context) : base(context)
    {
    }
}