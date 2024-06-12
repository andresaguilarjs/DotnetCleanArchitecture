namespace Application;

public record DeleteUserCommand(Guid Id) : ICommand;
