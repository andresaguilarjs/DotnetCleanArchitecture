using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using Domain.Entities.User.ValueObjects;
using Infrastructure.Database.Common;
using Infrastructure.Database.DBContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Repositories.Query;

public sealed class UserQueryRepository : QueryRepository<UserEntity>, IUserQueryRepository
{
    private readonly DbSet<UserEntity> _userEntity;

    public UserQueryRepository(ApplicationDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
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