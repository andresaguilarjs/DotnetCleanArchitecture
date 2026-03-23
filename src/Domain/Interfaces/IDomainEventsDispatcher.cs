namespace Domain.Interfaces;

/// <summary>
/// Defines a contract for dispatching domain events asynchronously.
/// </summary>
/// <remarks>Implementations of this interface are responsible for delivering domain events to their respective
/// handlers. The dispatching process is used to propagate changes or trigger side effects within a
/// domain-driven design context.</remarks>
public interface IDomainEventsDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
