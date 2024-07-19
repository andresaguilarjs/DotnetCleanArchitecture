using Domain.Abstractions;
using Domain.Common;

namespace Domain.Entities.User;

/// <summary>
/// Represents an error that occurred when working with a user.
/// </summary>
public class UserErrors
{
    public static Error EmptyEmail() => new Error("empty_email", "The email address is required.");
    public static Error InvalidEmail() => new Error("invalid_email", "The email address is invalid.");
}