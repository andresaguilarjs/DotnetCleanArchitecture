using Domain.Entities.User.ValueObjects;

namespace Application.Users.Commands.CreateUser;

public record CreateUserCommand(Email Email, FirstName FirstName, LastName LastName) : ICommand;