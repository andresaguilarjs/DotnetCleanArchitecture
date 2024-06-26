using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using Infrastructure.Database.Common;
using Infrastructure.Database.DBContext;

namespace Infrastructure.Database.Repositories;

public sealed class UserRepository : Repository<UserEntity>, IUserRepository
{

    public UserRepository(ApplicationDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
    {
    }
}