using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using MediatR;

namespace Application.Users.Commands.CreateUser;

internal sealed class CrateUserCommandHandler : ICommandHandler<CreateUserCommand, UserDTO>
{
    private readonly IUserRepository _userRepository;

    public CrateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDTO>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        UserEntity user = UserEntity.Create(request.Email, request.FirstName, request.LastName);

        await _userRepository.AddAsync(user);

        return Result<UserDTO>.Success(UserMapper.Map(user));
    }
}