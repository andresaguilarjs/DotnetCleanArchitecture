using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using MediatR;

namespace Application.Users.Commands.UpdateUser;

internal sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, UserDTO>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDTO>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        UserEntity? user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user is null)
        {
            return Result<UserDTO>.Failure( new List<Error>() { GenericErrors.NotFound(request.Id, typeof(UserDTO)) });
        }

        user.Update(request.Email, request.FirstName, request.LastName);

        await _userRepository.UpdateAsync(user);
        return Result<UserDTO>.Success(UserMapper.Map(user));
    }
}
