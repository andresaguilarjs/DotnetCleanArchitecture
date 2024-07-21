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

    public static Result<FirstName> Create(string firstName)
    {
        if (ValidateName(firstName, typeof(FirstName)).IsFailure)
        {
            return Result<FirstName>.Failure(ValidateName(firstName, typeof(FirstName)).Errors);
        }

        return Result<FirstName>.Success(new FirstName(firstName));
    }
}