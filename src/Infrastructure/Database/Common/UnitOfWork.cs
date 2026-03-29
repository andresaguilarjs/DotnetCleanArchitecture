
using Domain.Abstractions;
using Domain.Common;
using Domain.Interfaces;
using Infrastructure.Database.DBContext;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Database.Common;

public sealed class UnitOfWork(
    ApplicationDbContext context,
    ILogger<UnitOfWork> logger,
    IPublishEndpoint publishEndpoint
    ) : IUnitOfWork
{
    public async Task<Result> SaveChangesAsync(CancellationToken cancellationToken)
    {
        List<IDomainEvent> eventsToPublish = CollectDomainEventsFromTrackedEntities();
        try
        {
            foreach (IDomainEvent domainEvent in eventsToPublish)
            {
                await publishEndpoint.Publish(domainEvent, domainEvent.GetType(), cancellationToken);
            }

            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "Concurrency conflict occurred while saving changes to the database.");

            return Result.Failure(new Error(
                ErrorCode.Conflict,
                """
                The data may have been modified or deleted since it was last read.
                It is recommended to refresh the data and try again.
                """
            ));
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "An error occurred while saving changes to the database.");

            string message = ex.InnerException?.Message ?? ex.Message;
            if (message.Contains("UNIQUE") || message.Contains("duplicate key"))
            {
                return Result.Failure(new Error(
                    ErrorCode.Conflict,
                    "There was a conflict with the database. Please try again."
                ));
            }
            return Result.Failure(GenericErrors.SomethingWhenWrong());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while saving changes to the database.");
            return Result.Failure(GenericErrors.SomethingWhenWrong());
        }
        
        ClearDomainEventsOnTrackedEntities();

        return Result.Success();
    }

    /// <summary>
    /// Clears all domain events from tracked entities in the current context that are not in the detached state.
    /// </summary>
    /// <remarks>This method iterates over all tracked entities of type BaseEntity and removes any domain
    /// events they contain. Entities that are detached from the context are ignored.</remarks>
    private void ClearDomainEventsOnTrackedEntities()
    {
        foreach (EntityEntry<BaseEntity> entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State is not EntityState.Detached)
                entry.Entity.ClearDomainEvents();
        }
    }

    /// <summary>
    /// Collects all domain events from tracked entities that have been added, modified, or deleted in the current
    /// context.
    /// </summary>
    /// <remarks>Only entities of type BaseEntity that are in the Added, Modified, or Deleted state are
    /// considered. This method is used to gather domain events before committing changes to the
    /// database.</remarks>
    /// <returns>A list of domain events associated with entities that are being tracked for changes. The list is empty if no
    /// such events are found.</returns>
    private List<IDomainEvent> CollectDomainEventsFromTrackedEntities()
    {
        List<IDomainEvent> events = [];
        foreach (EntityEntry<BaseEntity> entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                events.AddRange(entry.Entity.DomainEvents);
        }
        return events;
    }
}
