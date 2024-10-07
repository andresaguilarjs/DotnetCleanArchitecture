using Domain.Abstractions;

namespace Domain.Entities.User;

/// <summary>
/// Represents a user read entity which is the read-only representation of a user entity.
/// </summary>
public class UserReadEntity : BaseReadEntity
{
    public string Email { get; init; } = default!;
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;

    public UserReadEntity(string email, string firstName, string lastName)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }
};
