using Domain.Abstractions;
using Domain.Common;

namespace Domain.Entities.User;

/// <summary>
/// Represents an error that occurred when working with a user.
/// </summary>
public class UserErrors
{
    public static Error NotFound(string email) => new Error(ErrorCodes.NotFound, $"The user with email '{email}' was not found.");
    public static Error EmptyName(string nameType) => new Error(ErrorCodes.ValidationError, $"The {nameType} is required.");
    public static Error InvalidNameLength(string nameType) => new Error(ErrorCodes.ValidationError, $"The {nameType} must be over 2 and less of 50 characters long.");
    public static Error EmptyEmail() => new Error(ErrorCodes.ValidationError, "The email address is required.");
    public static Error InvalidEmail() => new Error(ErrorCodes.ValidationError, "The email address is invalid.");
    public static Error EmailAlreadyInUse(string email) => new Error(ErrorCodes.ValidationError, $"The email address '{email}' is already in use.");
}