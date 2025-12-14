using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using Infrastructure.Database.Common;
using Infrastructure.Database.DBContext;

namespace Infrastructure.Database.Repositories.Command;

public sealed class UserCommandRepository : CommandRepository<UserEntity>, IUserCommandRepository
{
    public UserCommandRepository(ApplicationDbContext context) : base(context)
    {
        context.Set<UserEntity>();
    }
}