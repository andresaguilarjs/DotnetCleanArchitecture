using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities.User.ValueObjects;
using MediatR;

namespace Application.Users.Commands.UpdateUser;

public record UpdateUserCommand(Guid Id, Email Email, FirstName FirstName, LastName LastName) : ICommand;