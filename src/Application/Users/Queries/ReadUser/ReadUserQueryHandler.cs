﻿using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;

namespace Application.Users.Queries.ReadUser;

internal sealed class ReadUserQueryHandler : IQueryHandler<ReadUserQuery, UserDTO>
{
    private readonly IUserRepository _userRepository;

    public ReadUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDTO>> Handle(ReadUserQuery request, CancellationToken cancellationToken)
    {
        UserEntity? user = await _userRepository.GetByIdAsync(request.Id);

        if (user is null)
        {
            return UserErrors<UserDTO>.NotFound(request.Id);;
        }

        return Result<UserDTO>.Success(UserMapper.Map(user));
    }
}
