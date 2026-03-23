namespace Domain.Interfaces;

/// <summary>
/// Represents a domain event that signifies a occurrence within the domain model.
/// </summary>
/// <remarks>Implement this interface to define events that capture changes or actions within the domain. Domain
/// events are typically used to communicate state changes between different parts of the application or to trigger side
/// effects in response to business operations.</remarks>
public interface IDomainEvent;
