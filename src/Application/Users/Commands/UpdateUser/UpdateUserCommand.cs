using Domain.Common;
using Domain.Entities.User.ValueObjects;
using MediatR;

namespace Application;

public record UpdateUserCommand(Guid Id, Email Email, FirstName FirstName, LastName LastName) : ICommand;