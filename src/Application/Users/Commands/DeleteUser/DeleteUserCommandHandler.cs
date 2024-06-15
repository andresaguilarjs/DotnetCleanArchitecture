using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;

namespace Application.Users.Commands.DeleteUser;

internal sealed class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        UserEntity user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
        {
            return Result.Failure("The user was not found.");
        }

        user.Delete();
        await _userRepository.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }
}
