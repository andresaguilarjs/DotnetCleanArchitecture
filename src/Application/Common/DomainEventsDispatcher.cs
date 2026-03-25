using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common;

internal class DomainEventsDispatcher(IServiceProvider serviceProvider) : IDomainEventsDispatcher
{
    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceProvider.CreateScope();

        foreach (IDomainEvent domainEvent in domainEvents)
        {
            Type domainEventType = domainEvent.GetType();
            Type handlerType = typeof(IDomainEventHandler<>)
                .MakeGenericType(domainEventType);

            List<object> handlers = scope.ServiceProvider.GetServices(handlerType)
                .Cast<object>()
                .ToList();

            foreach (var handler in handlers)
            {
                dynamic h = handler;
                await h.Handle((dynamic)domainEvent, cancellationToken);
            }
        }
    }
}
