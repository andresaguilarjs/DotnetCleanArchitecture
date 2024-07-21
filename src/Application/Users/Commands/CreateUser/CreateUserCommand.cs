using Application.Abstractions.Messaging;
using Domain.Entities.User.ValueObjects;

namespace Application.Users.Commands.CreateUser;

public record CreateUserCommand(UserRequest UserRequest) : ICommand<UserDTO>;