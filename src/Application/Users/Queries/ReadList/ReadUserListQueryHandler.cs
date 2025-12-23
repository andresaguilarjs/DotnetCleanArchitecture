using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;

namespace Application.Users.Queries.ReadList;

public sealed class ReadUserListQueryHandler(IUserQueryRepository _userQueryRepository) 
    : IQueryHandler<ReadUserListQuery, IList<UserDTO>>
{
    public async Task<Result<IList<UserDTO>>> Handle(ReadUserListQuery request, CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<UserEntity>> users = await _userQueryRepository.ListAllAsync();

        if (users.IsFailure)
        {
            return Result<IList<UserDTO>>.Failure(users.Errors);
        }

        return Result<IList<UserDTO>>.Success(UserMapper.Map(users.Value.ToList()));
    }
}