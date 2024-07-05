using Application.Abstractions.Messaging;
using Domain.Entities.User;

namespace Application.Users.Queries.ReadList;

public class ReadUserListQuery : IQuery<IEnumerable<UserDTO>>;