using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;

namespace Application.Users.Queries.ReadUser;

internal sealed class ReadUserQueryHandler : IQueryHandler<ReadUserQuery, UserDTO>
{
    private readonly IUserQueryRepository _userQueryRepository;

    public ReadUserQueryHandler(IUserQueryRepository userRepository)
    {
        _userQueryRepository = userRepository;
    }

    public async Task<Result<UserDTO>> Handle(ReadUserQuery request, CancellationToken cancellationToken)
    {
        Result<UserEntity> user = await _userQueryRepository.GetByIdAsync(request.Id);

        if (user.IsFailure)
        {
            return Result<UserDTO>.Failure(user.Errors);
        }

        return Result<UserDTO>.Success(UserMapper.Map(user));
    }
}
