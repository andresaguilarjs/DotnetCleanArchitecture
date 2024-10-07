using Domain.Entities.Comment;
using Domain.Entities.Comment.Interfaces;
using Infrastructure.Database.Abstractions;
using Infrastructure.Database.Common;
using Infrastructure.Database.DBContext;

namespace Infrastructure.Database.Repositories;

public class CommentRepository : QueryRepository<CommentReadEntity>, ICommentRepository
{
    public CommentRepository(ApplicationReadDbContext context) : base(context)
    {
    }
}