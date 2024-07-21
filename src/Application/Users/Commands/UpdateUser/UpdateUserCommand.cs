using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.ValueObjects;
using MediatR;

namespace Application.Users.Commands.UpdateUser;

public record UpdateUserCommand(UserRequest UserRequest) : ICommand<UserDTO>;