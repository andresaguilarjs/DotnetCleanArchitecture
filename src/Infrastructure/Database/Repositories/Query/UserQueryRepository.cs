using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using Domain.Entities.User.ValueObjects;
using Infrastructure.Database.Abstractions;
using Infrastructure.Database.DBContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Repositories.Query;

public sealed class UserQueryRepository : QueryRepository<UserReadEntity>, IUserQueryRepository
{
    private readonly DbSet<UserReadEntity> _userEntity;

    public UserQueryRepository(ApplicationReadDbContext context) : base(context)
    {
        _userEntity = context.Set<UserReadEntity>();
    }

    public async Task<Result<UserReadEntity>> GetByEmailAsync(string email)
    {
        UserReadEntity? user = await _userEntity.FirstOrDefaultAsync(user => user.Email == email);

        if (user is null)
        {
            List<Error> errors = new List<Error> { UserErrors.NotFound(email) };
            return Result<UserReadEntity>.Failure(errors);
        }

        return Result<UserReadEntity>.Success(user);
    }
}