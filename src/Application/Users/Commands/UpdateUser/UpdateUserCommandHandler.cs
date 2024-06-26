﻿using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;

namespace Application.Users.Commands.UpdateUser;

internal sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        UserEntity? user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user is null)
        {
            return UserErrors.UserNotFound(request.Id);
        }

        user.Update(request.Email, request.FirstName, request.LastName);

        await _userRepository.UpdateAsync(user);
        return Result.Success();
    }
}
