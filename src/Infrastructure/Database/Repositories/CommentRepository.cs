using Domain.Entities.Comment;
using Domain.Entities.Comment.Interfaces;
using Infrastructure.Database.Common;
using Infrastructure.Database.DBContext;

namespace Infrastructure.Database.Repositories;

public class CommentRepository : Repository<CommentEntity>, ICommentRepository
{
    public CommentRepository(ApplicationDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
    {
    }
}