using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using Domain.Entities.User.ValueObjects;
using Infrastructure.Database.Abstractions;
using Infrastructure.Database.DBContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Repositories.Command;

public sealed class UserCommandRepository : CommandRepository<UserEntity>, IUserCommandRepository
{
    private readonly DbSet<UserEntity> _userEntity;

    public UserCommandRepository(ApplicationWriteDbContext context) : base(context)
    {
        _userEntity = context.Set<UserEntity>();
    }

    public async Task<Result<UserEntity>> GetByEmailAsync(string email)
    {
        UserEntity? user = await _userEntity.FirstOrDefaultAsync(user => user.Email.Value == email);

        if (user is null)
        {
            List<Error> errors = new List<Error> { UserErrors.NotFound(email) };
            return Result<UserEntity>.Failure(errors);
        }

        return Result<UserEntity>.Success(user);
    }
}