using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Users.Events;

internal class SendWelcomeEmailHandler(ILogger<SendWelcomeEmailHandler> logger) : IDomainEventHandler<UserRegisteredDomainEvent>
{
    private readonly ILogger<SendWelcomeEmailHandler> _logger = logger;
    public Task Handle(UserRegisteredDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending welcome email to user with ID: {UserId}", domainEvent.UserId);
        return Task.CompletedTask;
    }
}
