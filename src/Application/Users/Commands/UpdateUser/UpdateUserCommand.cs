using Application.Abstractions.Messaging;

namespace Application.Users.Commands.UpdateUser;

public record UpdateUserCommand(UserRequest UserRequest) : ICommand<UserDTO>;