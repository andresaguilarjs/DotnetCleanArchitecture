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

    public static Result<Email> Create(string email) {
        if (string.IsNullOrWhiteSpace(email)) {
            return EmptyEmail();
        }

        if (!IsValid(email)) {
            return InvalidEmail();
        }

        return Result<Email>.Success(new Email(email));
    }

    public static bool IsValid(string email) {
        return new EmailAddressAttribute().IsValid(email);
    }

    private static Result<Email> InvalidEmail() {
        return Result<Email>.Failure(
            new List<Error>() {
                UserErrors.InvalidEmail()
            }
        );
    }

    private static Result<Email> EmptyEmail() {
        return Result<Email>.Failure(
            new List<Error>() {
                UserErrors.EmptyEmail()
            }
        );
    }
};
