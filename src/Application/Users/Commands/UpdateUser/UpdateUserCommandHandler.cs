﻿using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using Domain.Entities.User.Services;
using Domain.Entities.User.ValueObjects;
using MediatR;

namespace Application.Users.Commands.UpdateUser;

internal sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, UserDTO>
{
    private readonly IUserCommandRepository _userCommandRepository;
    private readonly IUserQueryRepository _userQueryRepository;
    private readonly UserService _userService;

    public UpdateUserCommandHandler(IUserCommandRepository userRepository, IUserQueryRepository userQueryRepository, UserService userService)
    {
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

        user = _userService.UpdateUserEntity(user, request.UserRequest.Email, request.UserRequest.FirstName, request.UserRequest.LastName);

        if (user.IsFailure)
        {
            return Result<UserDTO>.Failure(user.Errors);
        }

        await _userCommandRepository.UpdateAsync(user);

        return Result<UserDTO>.Success(UserMapper.Map(user));
    }
}
