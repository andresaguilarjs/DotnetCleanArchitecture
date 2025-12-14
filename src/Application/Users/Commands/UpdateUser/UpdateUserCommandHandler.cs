using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using Domain.Interfaces;

namespace Application.Users.Commands.UpdateUser;

internal sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, UserDTO>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserCommandRepository _userCommandRepository;
    private readonly IUserQueryRepository _userQueryRepository;
    private readonly IUserService _userService;

    public UpdateUserCommandHandler(IUnitOfWork unitOfWork, IUserCommandRepository userRepository, IUserQueryRepository userQueryRepository, IUserService userService)
    {
        _unitOfWork = unitOfWork;
        _userCommandRepository = userRepository;
        _userQueryRepository = userQueryRepository;
        _userService = userService;
    }

    public async Task<Result<UserDTO>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        if (request.UserRequest.Id is null)
        {
            return Result<UserDTO>.Failure(new List<Error>() { GenericErrors.NullOrEmpty(nameof(request.UserRequest)) });
        }

        Result<UserEntity> user = await _userQueryRepository.GetByIdAsync((Guid)request.UserRequest.Id, cancellationToken);

        if (user.IsFailure)
        {
            return Result<UserDTO>.Failure( new List<Error>() { GenericErrors.NotFound((Guid)request.UserRequest.Id, typeof(UserDTO)) });
        }

        user = await _userService.UpdateUserEntityAsync(user, request.UserRequest.Email, request.UserRequest.FirstName, request.UserRequest.LastName);

        if (user.IsFailure)
        {
            return Result<UserDTO>.Failure(user.Errors);
        }

        Result result = await _userCommandRepository.UpdateAsync(user, cancellationToken);
        if (result.IsFailure)
        {
            return Result<UserDTO>.Failure(result.Errors);
        }

        Result saveChangesResult = await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            return Result<UserDTO>.Failure(saveChangesResult.Errors);
        }

        return Result<UserDTO>.Success(UserMapper.Map(user));
    }
}
