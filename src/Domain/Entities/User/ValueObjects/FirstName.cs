using Domain.Abstractions.ValueObjects;
using Domain.Common;

namespace Domain.Entities.User.ValueObjects;

/// <summary>
/// Represents the first name of a user.
/// </summary>
public record FirstName : Name
{
    private FirstName(string value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new <see cref="FirstName"/> instance after validating the specified first name.
    /// </summary>
    /// <param name="firstName">The first name to validate and use for creating the <see cref="FirstName"/> instance. Cannot be null or empty.</param>
    /// <returns>A <see cref="Result{FirstName}"/> that represents the outcome of the operation. Returns a successful result
    /// containing the new <see cref="FirstName"/> if validation succeeds; otherwise, returns a failure result with
    /// validation errors.</returns>
    public static Result<FirstName> Create(string firstName)
    {
        if (ValidateName(firstName, typeof(FirstName)).IsFailure)
        {
            return Result<FirstName>.Failure(ValidateName(firstName, typeof(FirstName)).Errors);
        }

        return Result<FirstName>.Success(new FirstName(firstName));
    }

    /// <summary>
    /// Creates a new instance of the FirstName class from a value retrieved from the database.
    /// </summary>
    /// <param name="value">The string value representing the first name as stored in the database. Cannot be null, empty, or consist only
    /// of white-space characters.</param>
    /// <returns>A FirstName instance initialized with the specified value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if value is null, empty, or consists only of white-space characters.</exception>
    public static FirstName CreateFromDatabase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("FirstName value from database cannot be null or empty.");
        }
        return new FirstName(value);
    }
}