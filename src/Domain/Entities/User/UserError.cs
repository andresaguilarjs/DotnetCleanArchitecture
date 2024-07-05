using Domain.Common;

namespace Domain.Entities.User;

public static class UserErrors<T>
{
    public static Error<T> NotFound(Guid userId) => new Error<T>("user_not_found", $"The user with ID '{userId}' was not found.");
}