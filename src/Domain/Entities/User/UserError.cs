using Domain.Common;

namespace Domain.Entities.User;

public static class UserErrors
{
    public static Error UserNotFound(Guid userId) => new Error("user_not_found", $"The user with ID '{userId}' was not found.");
}