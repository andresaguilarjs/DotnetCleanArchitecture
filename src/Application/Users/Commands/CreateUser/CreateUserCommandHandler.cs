using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using Domain.Entities.User.ValueObjects;
using Domain.Interfaces;
using MediatR;

namespace Application.Users.Commands.CreateUser;

public sealed class CrateUserCommandHandler : ICommandHandler<CreateUserCommand, UserDTO>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserCommandRepository _userCommandRepository;
    private readonly IUserService _userService;

    public CrateUserCommandHandler(IUnitOfWork unitOfWork, IUserCommandRepository userRepository, IUserService userService)
    {
        _unitOfWork = unitOfWork;
        _userCommandRepository = userRepository;
        _userService = userService;
    }

    public async Task<Result<UserDTO>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        Result<UserEntity> user = _userService.CreateUserEntity(request.UserRequest.Email, request.UserRequest.FirstName, request.UserRequest.LastName);

        if (user.IsFailure)
        {
            return Result<UserDTO>.Failure(user.Errors);
        }

        await _userCommandRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UserDTO>.Success(UserMapper.Map(user));
    }
}