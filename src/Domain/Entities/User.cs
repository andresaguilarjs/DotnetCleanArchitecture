using Domain.Common;
using Domain.ValueObjects;

namespace Domain;

public class User : BaseEntity
{
    public Username Username { get; set; } = default!;
    public Email Email { get; set; } = default!;
    public FirstName FirstName { get; set; } = default!;
    public LastName LastName { get; set; } = default!;
}
