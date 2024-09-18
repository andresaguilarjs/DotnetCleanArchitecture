using Domain.Entities.Comment;
using Domain.Entities.Comment.Interfaces;
using Infrastructure.Database.Common;
using Infrastructure.Database.DBContext;

namespace Infrastructure.Database.Repositories;

public class CommentRepository : QueryRepository<CommentEntity>, ICommentRepository
{
    public CommentRepository(ApplicationDbContext context) : base(context)
    {
    }
}