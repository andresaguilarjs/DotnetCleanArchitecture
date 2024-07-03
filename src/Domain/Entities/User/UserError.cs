using Domain.Common;

namespace Domain.Entities.User;

public static class UserErrors
{
    public static Error<UserEntity> NotFound(Guid userId) => new Error<UserEntity>("user_not_found", $"The user with ID '{userId}' was not found.");
}