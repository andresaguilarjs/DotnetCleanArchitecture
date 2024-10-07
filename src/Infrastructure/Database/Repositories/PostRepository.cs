using Domain.Entities.Post;
using Domain.Entities.Post.Interfaces;
using Infrastructure.Database.Abstractions;
using Infrastructure.Database.DBContext;

namespace Infrastructure.Database.Repositories;

public class PostRepository : QueryRepository<PostReadEntity>, IPostRepository
{
    public PostRepository(ApplicationReadDbContext context) : base(context)
    {
    }
}
