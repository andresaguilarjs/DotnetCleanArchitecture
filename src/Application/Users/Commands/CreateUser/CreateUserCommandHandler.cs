using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using MediatR;

namespace Application.Users.Commands.CreateUser;

internal sealed class CrateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    private readonly IUserRepository _userRepository;

    public CrateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new UserEntity(){
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        await _userRepository.AddAsync(user);

        return Result.Success();
    }
}