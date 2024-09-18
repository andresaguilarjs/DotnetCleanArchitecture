using Domain.Entities.Post;
using Domain.Entities.Post.Interfaces;
using Infrastructure.Database.Common;
using Infrastructure.Database.DBContext;

namespace Infrastructure.Database.Repositories;

public class PostRepository : QueryRepository<PostEntity>, IPostRepository
{
    public PostRepository(ApplicationDbContext context) : base(context)
    {
    }
}
