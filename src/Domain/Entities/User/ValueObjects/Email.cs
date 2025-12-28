using System.ComponentModel.DataAnnotations;
using Domain.Common;

namespace Domain.Entities.User.ValueObjects;

/// <summary>
/// Represents an email address.
/// </summary>
public record Email {
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="Email"/> instance from the specified email address, validating its format and content.
    /// </summary>
    /// <param name="email">The email address to validate and use for creating the <see cref="Email"/> instance. Cannot be null, empty, or
    /// consist only of white-space characters.</param>
    /// <returns>A <see cref="Result{Email}"/> indicating the outcome of the operation. Returns a successful result containing
    /// the <see cref="Email"/> if the address is valid; otherwise, returns a failure result describing the validation
    /// error.</returns>
    public static Result<Email> Create(string email) {
        if (string.IsNullOrWhiteSpace(email)) {
            return EmptyEmail();
        }

        if (!IsValid(email)) {
            return InvalidEmail();
        }

        return Result<Email>.Success(new Email(email));
    }

    /// <summary>
    /// Creates a placeholder <see cref="Email"/> instance for a deleted entity using its unique identifier.
    /// </summary>
    /// <param name="entityId">Guid of the entity where this email belongs to</param>
    /// <returns>Returns and Email object </returns>
    public static Email CreateForDeleted(Guid entityId)
    {
        return new Email($"{entityId}@deleted.local");
    }

    /// <summary>
    /// Creates a new <see cref="Email"/> instance from a value retrieved from the database without additional validation.
    /// </summary>
    /// <param name="value">Value of the Email</param>
    /// <returns>A Email instance initialized with the specified value.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static Email CreateFromDatabase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("Email value from database cannot be null or empty.");
        }
        return new Email(value);
    }

    /// <summary>
    /// Checks if the provided email address is in a valid format.
    /// </summary>
    /// <param name="email">Email address as a string to validate</param>
    /// <returns>true if it's a validid address, otherwise it will return false</returns>
    public static bool IsValid(string email) {
        return new EmailAddressAttribute().IsValid(email);
    }

    /// <summary>
    /// Creates a failure result indicating that the provided email address is invalid.
    /// </summary>
    /// <returns>Returns a failure result indicating that the provided email address is invalid</returns>
    private static Result<Email> InvalidEmail() {
        return Result<Email>.Failure(
            new List<Error>() {
                UserErrors.InvalidEmail()
            }
        );
    }

    /// <summary>
    /// Creates a failure result indicating that the provided email address is empty.
    /// </summary>
    /// <returns>Returns a failure result indicating that the provided email address is Empty</returns>
    private static Result<Email> EmptyEmail() {
        return Result<Email>.Failure(
            new List<Error>() {
                UserErrors.EmptyEmail()
            }
        );
    }
};
