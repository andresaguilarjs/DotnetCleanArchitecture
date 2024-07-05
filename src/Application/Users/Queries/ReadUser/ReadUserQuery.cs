using Application.Abstractions.Messaging;
using Domain.Entities.User;

namespace Application.Users.Queries.ReadUser;

public record ReadUserQuery(Guid Id) : IQuery<UserDTO>;