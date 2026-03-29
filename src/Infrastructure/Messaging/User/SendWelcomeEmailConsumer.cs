using Domain.Interfaces;
using Domain.Entities.Users.Events;
using MassTransit;

namespace Infrastructure.Messaging.User;

public class SendWelcomeEmailConsumer
    (IDomainEventHandler<UserRegisteredDomainEvent> domainEventHandler)
    : IConsumer<UserRegisteredDomainEvent>
{
    public Task Consume(ConsumeContext<UserRegisteredDomainEvent> context)
    {
        return domainEventHandler.Handle(context.Message, context.CancellationToken);
    }
}
