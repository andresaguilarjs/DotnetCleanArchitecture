﻿using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;

namespace Application.Users.Queries.ReadUser;

internal sealed class ReadUserQueryHandler : IQueryHandler<ReadUserQuery, UserEntity>
{
    private readonly IUserRepository _userRepository;

    public ReadUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserEntity>> Handle(ReadUserQuery request, CancellationToken cancellationToken)
    {
        UserEntity? user = await _userRepository.GetByIdAsync(request.Id);

        if (user is null)
        {
            return Result<UserEntity>.Failure(UserErrors.UserNotFound(request.Id));
        }

        return Result<UserEntity>.Success(user);
    }
}
