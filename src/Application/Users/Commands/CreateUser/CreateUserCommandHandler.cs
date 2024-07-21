using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using Domain.Entities.User.Services;
using Domain.Entities.User.ValueObjects;
using MediatR;

namespace Application.Users.Commands.CreateUser;

internal sealed class CrateUserCommandHandler : ICommandHandler<CreateUserCommand, UserDTO>
{
    private readonly IUserRepository _userRepository;
    private readonly UserService _userService;

    public CrateUserCommandHandler(IUserRepository userRepository, UserService userService)
    {
        _userRepository = userRepository;
        _userService = userService;
    }

    public async Task<Result<UserDTO>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        Result<UserEntity> user = _userService.CreateUserEntity(request.UserRequest.Email, request.UserRequest.FirstName, request.UserRequest.LastName);

        if (user.IsFailure)
        {
            return Result<UserDTO>.Failure(user.Errors);
        }

        await _userRepository.AddAsync(user);

        return Result<UserDTO>.Success(UserMapper.Map(user));
    }
}