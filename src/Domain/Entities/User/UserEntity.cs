using Domain.Common;
using Domain.Entities.User.ValueObjects;

namespace Domain.Entities.User;

public class UserEntity : BaseEntity
{
    public Email Email { get; set; } = default!;
    public FirstName FirstName { get; set; } = default!;
    public LastName LastName { get; set; } = default!;
}
