using Domain.Entities.Post;
using Domain.Entities.Post.Interfaces;
using Infrastructure.Database.Common;

namespace Infrastructure.Database.Repositories;

public class PostRepository : Repository<PostEntity>, IPostRepository
{
    public PostRepository(ApplicationDbContext context) : base(context)
    {
    }
}
