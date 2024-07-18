using Domain.Common;

namespace Domain.Entities.User;

/// <summary>
/// Represents an error that occurred when working with a user.
/// </summary>
/// <typeparam name="T"></typeparam>
public static class UserErrors<T>
{
    /// <summary>
    /// Returns an error indicating that the user was not found.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public static Error<T> NotFound(Guid userId) => new Error<T>("user_not_found", $"The user with ID '{userId}' was not found.");

    public static Error<T> EmptyEmail() => new Error<T>("empty_email", "The email address is required.");
    public static Error<T> InvalidEmail() => new Error<T>("invalid_email", "The email address is invalid.");
}