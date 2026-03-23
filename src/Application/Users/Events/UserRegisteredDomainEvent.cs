using Domain.Interfaces;

namespace Application.Users.Events;

internal sealed record UserRegisteredDomainEvent : IDomainEvent
{
    public Guid UserId { get; init; }
}
