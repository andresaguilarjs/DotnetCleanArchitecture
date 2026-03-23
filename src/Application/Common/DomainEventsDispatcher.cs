using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;

namespace Application.Common;

internal class DomainEventsDispatcher(IServiceProvider serviceProvider) : IDomainEventsDispatcher
{
    private readonly ConcurrentDictionary<(Type, Type), Type> _handlerTypeCache = new();
    private readonly ConcurrentDictionary<Type, MethodInfo> _handleMethodCache = new();

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            using IServiceScope scope = serviceProvider.CreateScope();

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
