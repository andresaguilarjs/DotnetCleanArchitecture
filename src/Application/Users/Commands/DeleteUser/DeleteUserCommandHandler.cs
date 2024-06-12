using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;

namespace Application;

internal sealed class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        UserEntity? user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user == null)
        {
            return Result.Failure("User not found.");
        }

        await _userRepository.DeleteAsync(user);
        return Result.Success();
    }
}
