using Domain.Abstractions;
using Domain.Entities.User.ValueObjects;

namespace Domain.Entities.User;

/// <summary>
/// Represents a user entity.
/// </summary>
public sealed class UserEntity : BaseEntity
{
    /// <summary>
    /// Gets or sets the email of the user.
    /// </summary>
    public Email Email { get; private set; } = default!;

    /// <summary>
    /// Gets or sets the first name of the user.
    /// </summary>
    public FirstName FirstName { get; private set; } = default!;

    /// <summary>
    /// Gets or sets the last name of the user.
    /// </summary>
    public LastName LastName { get; private set; } = default!;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserEntity"/> class.
    /// </summary>
    private UserEntity(Email email, FirstName firstName, LastName lastName) : base()
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="UserEntity"/> class.
    /// </summary>
    /// <param name="email">The email of the user.</param>
    /// <param name="firstName">The first name of the user.</param>
    /// <param name="lastName">The last name of the user.</param>
    /// <returns>A new instance of the <see cref="UserEntity"/> class.</returns>
    public static UserEntity Create(Email email, FirstName firstName, LastName lastName)
    {
        return new UserEntity(email, firstName, lastName);
    }

    /// <summary>
    /// Updates the user entity.
    /// </summary>
    /// <param name="email">The email of the user.</param>
    /// <param name="firstName">The first name of the user.</param>
    /// <param name="lastName">The last name of the user.</param>
    public void Update(Email email, FirstName firstName, LastName lastName)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        base.RefreshUpdateAt();
    }
}
