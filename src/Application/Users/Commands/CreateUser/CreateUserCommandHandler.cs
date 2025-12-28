using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using Domain.Interfaces;

namespace Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler(
    IUnitOfWork unitOfWork,
    IUserCommandRepository userCommandRepository,
    IUserService userService) : ICommandHandler<CreateUserCommand, UserDTO>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserCommandRepository _userCommandRepository = userCommandRepository;
    private readonly IUserService _userService = userService;

    public async Task<Result<UserDTO>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        Result<UserEntity> user = await _userService.CreateUserEntityAsync(request.UserRequest.Email, request.UserRequest.FirstName, request.UserRequest.LastName);

        if (user.IsFailure)
        {
            return Result<UserDTO>.Failure(user.Errors);
        }

        await _userCommandRepository.AddAsync(user);
        Result result = await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (result.IsFailure)
        {
            return Result<UserDTO>.Failure(result.Errors);
        }

        return Result<UserDTO>.Success(UserMapper.Map(user));
    }
}