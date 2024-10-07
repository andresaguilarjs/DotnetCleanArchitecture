using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using Domain.Entities.User.ValueObjects;
using Domain.Interfaces;
using MediatR;

namespace Application.Users.Commands.UpdateUser;

internal sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, UserDTO>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserCommandRepository _userCommandRepository;
    private readonly IUserService _userService;

    public UpdateUserCommandHandler(IUnitOfWork unitOfWork, IUserCommandRepository userRepository, IUserService userService)
    {
        _unitOfWork = unitOfWork;
        _userCommandRepository = userRepository;
        _userService = userService;
    }

    public async Task<Result<UserDTO>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        if (request.UserRequest.Id is null)
        {
            return Result<UserDTO>.Failure(new List<Error>() { GenericErrors.NullOrEmpty(nameof(request.UserRequest)) });
        }

        Result<UserEntity> user = await _userCommandRepository.GetByIdAsync((Guid)request.UserRequest.Id, cancellationToken);

        if (user.IsFailure)
        {
            return Result<UserDTO>.Failure( new List<Error>() { GenericErrors.NotFound((Guid)request.UserRequest.Id, typeof(UserDTO)) });
        }

        user = _userService.UpdateUserEntity(user.Value, request.UserRequest.Email, request.UserRequest.FirstName, request.UserRequest.LastName);

        if (user.IsFailure)
        {
            return Result<UserDTO>.Failure(user.Errors);
        }

        _userCommandRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        UserDTO userDTO = new UserDTO(user.Value.Id, user.Value.Email.Value, user.Value.FirstName.Value, user.Value.LastName.Value);
        return Result<UserDTO>.Success(userDTO);
    }
}
