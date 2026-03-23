namespace Domain.Interfaces;

/// <summary>
/// Defines a handler for processing domain events of a specified type.
/// </summary>
/// <remarks>Implement this interface to provide custom logic for handling specific domain events within a
/// domain-driven design context. Handlers are typically invoked by the event dispatcher when a matching event is
/// published.</remarks>
/// <typeparam name="TDomainEvent">The type of domain event to handle. Must implement the IDomainEvent interface.</typeparam>
public interface IDomainEventHandler<in TDomainEvent> 
    where TDomainEvent : IDomainEvent
{
    Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
