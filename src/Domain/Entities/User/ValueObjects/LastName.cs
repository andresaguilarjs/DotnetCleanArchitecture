using Domain.Abstractions.ValueObjects;
using Domain.Common;

namespace Domain.Entities.User.ValueObjects;

/// <summary>
/// Represents the last name of a user.
/// </summary>
public sealed record LastName : Name
{
    private LastName(string value) : base(value)
    {
    }

    /// <summary>
    /// Creates a new instance of the LastName value object after validating the specified last name.
    /// </summary>
    /// <param name="lastName">The last name to validate and encapsulate. Cannot be null or empty.</param>
    /// <returns>A Result containing the LastName instance if validation succeeds; otherwise, a failure Result with validation
    /// errors.</returns>
    public static Result<LastName> Create(string lastName)
    {
        if (ValidateName(lastName, typeof(LastName)).IsFailure)
        {
            return Result<LastName>.Failure(ValidateName(lastName, typeof(LastName)).Errors);
        }

        return Result<LastName>.Success(new LastName(lastName));
    }

    /// <summary>
    /// Creates a new instance of the LastName class from a value retrieved from the database.
    /// </summary>
    /// <param name="value">The last name value obtained from the database. Cannot be null, empty, or consist only of white-space
    /// characters.</param>
    /// <returns>A LastName instance initialized with the specified value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if value is null, empty, or consists only of white-space characters.</exception>
    /// <remarks>
    /// Do not use this method for creating LastName instances from user input.
    /// It is intended solely for database retrieval scenarios.
    /// </remarks>
    public static LastName CreateFromDatabase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("LastName value from database cannot be null or empty.");
        }
        return new LastName(value);
    }
}