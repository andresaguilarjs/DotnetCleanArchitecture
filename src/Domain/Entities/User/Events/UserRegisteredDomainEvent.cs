using Domain.Interfaces;

namespace Domain.Entities.Users.Events;

public sealed record UserRegisteredDomainEvent : IDomainEvent
{
    public Guid UserId { get; init; }
}
