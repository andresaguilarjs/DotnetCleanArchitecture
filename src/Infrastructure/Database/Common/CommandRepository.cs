using Domain.Abstractions;
using Domain.Common;
using Domain.Interfaces;
using Infrastructure.Database.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Database.Common;

public class CommandRepository<TEntity> : ICommandRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly ApplicationDbContext Context;

    public CommandRepository(ApplicationDbContext context)
    {
        Context = context;
    }

    public async Task<Result<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await Context.Set<TEntity>().AddAsync(entity, cancellationToken);
        return Result<TEntity>.Success(entity);
    }

    public async Task<Result> UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        Result<TEntity> tracked = await GetTrackedEntityAsync(entity, cancellationToken);
        if (tracked.IsFailure) return Result.Failure(tracked.Errors);

        Context.Entry(tracked.Value).CurrentValues.SetValues(entity);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        TEntity? entity = await Context.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity == null) return Result.Failure(GenericErrors.NotFound(id, typeof(TEntity)));

        entity.Delete();
        return Result.Success();
    }

    private async Task<Result<TEntity>> GetTrackedEntityAsync(TEntity entity, CancellationToken cancellationToken)
    {
        EntityEntry<TEntity> entry = Context.Entry(entity);
        if (entry.State != EntityState.Detached) return Result<TEntity>.Success(entity);

        TEntity? tracked = await Context.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == entity.Id && !x.IsDeleted, cancellationToken);
        if (tracked == null) return Result<TEntity>.Failure(GenericErrors.NotFound(entity.Id, typeof(TEntity)));

        return Result<TEntity>.Success(tracked);
    }
}