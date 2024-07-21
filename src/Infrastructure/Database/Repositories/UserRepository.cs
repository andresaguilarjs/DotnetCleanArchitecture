using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using Domain.Entities.User.ValueObjects;
using Infrastructure.Database.Common;
using Infrastructure.Database.DBContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Repositories;

public sealed class UserRepository : Repository<UserEntity>, IUserRepository
{
    private readonly DbSet<UserEntity> _userEntity;

    public UserRepository(ApplicationDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
    {
        _userEntity = context.Set<UserEntity>();
    }

    public async Task<Result<UserEntity>> GetByEmailAsync(Email email)
    {
        UserEntity? user = await _userEntity.FirstOrDefaultAsync(user => user.Email == email);

        if (user is null)
        {
            List<Error> errors = new List<Error> { UserErrors.NotFound(email.Value) };
            return Result<UserEntity>.Failure(errors);
        }

        return Result<UserEntity>.Success(user);
    }
}