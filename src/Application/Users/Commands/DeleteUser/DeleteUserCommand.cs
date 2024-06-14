using Application.Abstractions.Messaging;

namespace Application.Users.Commands.DeleteUser;

public record DeleteUserCommand(Guid Id) : ICommand;
