using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using MediatR;

namespace Application.Users.Queries.ReadList;

internal sealed class ReadUserListQueryHandler : IRequestHandler<ReadUserListQuery, Result<IEnumerable<UserDTO>>>
{
    private readonly IUserRepository _userRepository;

    public ReadUserListQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<IEnumerable<UserDTO>>> Handle(ReadUserListQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<UserEntity> users = await _userRepository.ListAllAsync();
        return Result<IEnumerable<UserDTO>>.Success(UserMapper.Map(users));
    }
}