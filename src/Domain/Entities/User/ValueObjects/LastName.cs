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

    public static Result<LastName> Create(string lastName)
    {
        if (ValidateName(lastName, typeof(LastName)).IsFailure)
        {
            return Result<LastName>.Failure(ValidateName(lastName, typeof(LastName)).Errors);
        }

        return Result<LastName>.Success(new LastName(lastName));
    }
}