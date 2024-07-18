using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using MediatR;

namespace Application.Users.Queries.ReadList;

internal sealed class ReadUserListQueryHandler : IRequestHandler<ReadUserListQuery, Result<IList<UserDTO>>>
{
    private readonly IUserRepository _userRepository;

    public ReadUserListQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<IList<UserDTO>>> Handle(ReadUserListQuery request, CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<UserEntity>> users = await _userRepository.ListAllAsync();

        if (users.IsFailure)
        {
            return Result<IList<UserDTO>>.Failure(UserMapper.Map(users.Errors).ToList());
        }

        return Result<IList<UserDTO>>.Success(UserMapper.Map(users.Value.ToList()));
    }
}