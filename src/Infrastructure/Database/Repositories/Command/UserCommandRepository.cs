using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using Domain.Entities.User.ValueObjects;
using Infrastructure.Database.Common;
using Infrastructure.Database.DBContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Repositories.Command;

public sealed class UserCommandRepository : CommandRepository<UserEntity>, IUserCommandRepository
{
    private readonly DbSet<UserEntity> _userEntity;

    public UserCommandRepository(ApplicationDbContext context, IUnitOfWork unitOfWork, IUserQueryRepository queryRepository) : base(context, unitOfWork, queryRepository)
    {
        _userEntity = context.Set<UserEntity>();
    }
}