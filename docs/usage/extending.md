# Extending the project

This guide points to the patterns and docs you use when adding features. For deeper detail, see the linked architecture and example pages.

## Add a new use case (command or query)

1. **Command or query**: Add types under `Application/{Feature}/Commands/` or `Application/{Feature}/Queries/` following existing user features.
2. **Handler**: Implement `ICommandHandler<,>` or `IQueryHandler<,>` — handlers are discovered by Scrutor (see [Design Patterns: Mediator Pattern](../architecture/design_patterns.md#2-mediator-pattern)).
3. **Endpoint**: Add a FastEndpoints class under `WebApi/Endpoints/{Feature}/` and send the command or query through `IMediator`.
4. **Validation**: Optional `IValidator<T>` for the command or query; see [Examples: Creating a Validator](./examples.md#creating-a-validator).

See [Code Examples](./examples.md) for step-by-step samples.

## Add a domain event

Use this when a write operation should trigger side effects (notifications, integration, etc.) without bloating the command handler.

1. **Contracts** are already in Domain (`IDomainEvent`, `IDomainEventHandler<>`). Add a **record** implementing `IDomainEvent` under **`Domain/Entities/{Aggregate}/Events/`** (or another Domain folder that fits your model).
2. **Handler**: Implement `IDomainEventHandler<TYourEvent>` under **`Application/{Feature}/Events/`**. It is registered automatically by **Scrutor** (same scan as command/query handlers).
3. **Consumer** (Infrastructure): Add an `IConsumer<TYourEvent>` in `Infrastructure/Messaging/...` that resolves `IDomainEventHandler<TYourEvent>` and calls `Handle`. Consumers are registered by MassTransit (`AddConsumers` on the Infrastructure assembly in `Infrastructure/DependencyInjection.cs`).
4. **Raise events**: From the command handler, call `RaiseDomainEvent` on the relevant `BaseEntity` **before** `IUnitOfWork.SaveChangesAsync`. Do not inject `IPublishEndpoint` in the handler.
5. **Persistence**: `UnitOfWork.SaveChangesAsync` collects events from tracked entities, publishes via MassTransit (EF transactional outbox), then saves so data and outbox rows commit together.
6. **EF** (for new mapped entities): In `Infrastructure/Database/Configurations/`, add `builder.Ignore(x => x.DomainEvents)` so domain events are not persisted.

Full walkthrough: [Examples: Domain Events](./examples.md#domain-events). Behavior and caveats: [Design Patterns: Domain Events](../architecture/design_patterns.md#11-domain-events).

## Add persistence for a new entity

1. Define the entity and any domain interfaces in **Domain**.
2. Add EF configuration, repository implementations, and migrations in **Infrastructure**; register services in Infrastructure/WebApi DI as needed.
3. See [Architecture Layers](../architecture/layers.md) for the split between command/query repositories and `IUnitOfWork`.

## Layer rules

Dependencies must point inward: Application depends only on Domain; Infrastructure implements Domain interfaces. See [Layers](../architecture/layers.md) and [Overview](../architecture/overview.md).
