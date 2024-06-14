using Domain.Abstractions;
using Domain.Entities.User.ValueObjects;

namespace Domain.Entities.User;

/// <summary>
/// Represents a user entity.
/// </summary>
public class UserEntity : BaseEntity
{
    /// <summary>
    /// Gets or sets the email of the user.
    /// </summary>
    public Email Email { get; set; } = default!;

    /// <summary>
    /// Gets or sets the first name of the user.
    /// </summary>
    public FirstName FirstName { get; set; } = default!;

    /// <summary>
    /// Gets or sets the last name of the user.
    /// </summary>
    public LastName LastName { get; set; } = default!;
}
