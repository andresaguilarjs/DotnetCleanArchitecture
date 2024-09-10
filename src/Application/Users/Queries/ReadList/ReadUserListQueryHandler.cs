using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using MediatR;

namespace Application.Users.Queries.ReadList;

internal sealed class ReadUserListQueryHandler : IRequestHandler<ReadUserListQuery, Result<IList<UserDTO>>>
{
    private readonly IUserQueryRepository _userQueryRepository;

    public ReadUserListQueryHandler(IUserQueryRepository userRepository)
    {
        _userQueryRepository = userRepository;
    }

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