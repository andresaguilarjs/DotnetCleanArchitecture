using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;

namespace Application.Users.Commands.ReadUser;

internal sealed class ReadUserQueryHandler : IQueryHandler<ReadUserQuery, UserEntity>
{
    private readonly IUserRepository _userRepository;

    public ReadUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserEntity>> Handle(ReadUserQuery request, CancellationToken cancellationToken)
    {
        UserEntity? user = await _userRepository.GetByIdAsync(request.Id);

        if (user is null)
        {
            return Result<UserEntity>.Failure("User not found.");
        }

        return Result<UserEntity>.Success(user);
    }
}
