using Application.Abstractions.Messaging;
using Domain.Entities.User;

namespace Application;

public record ReadUserQuery(Guid Id) : IQuery<UserEntity>;