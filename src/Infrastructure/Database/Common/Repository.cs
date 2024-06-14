using Domain.Common;
using Infrastructure.Database.DBContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Common;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly ApplicationDbContext Context;
    protected readonly IUnitOfWork UnitOfWork;
    public Repository(ApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        Context = context;
        UnitOfWork = unitOfWork;
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Context.Set<TEntity>().AddAsync(entity);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        TEntity currentEntity = await Context.Set<TEntity>()
            .FirstOrDefaultAsync(x => x.Id == entity.Id)
            ?? throw new Exception($"{typeof(TEntity).Name} with {entity.Id} was not found.");

        Context.Entry(currentEntity).CurrentValues.SetValues(entity);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return currentEntity;
    }

    public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<TEntity> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await Context.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (result is null)
        {
            throw new Exception($"{typeof(TEntity).Name} with {id} was not found.");
        }

        return result;
    }

    public Task<IReadOnlyList<TEntity>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }


}