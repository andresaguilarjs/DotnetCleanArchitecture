using Domain.Abstractions;
using Domain.Common;
using Domain.Entities.User;
using Domain.Interfaces;
using Infrastructure.Database.Abstractions.Common;
using Infrastructure.Database.DBContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Abstractions;

public abstract class CommandRepository<TEntity> : GetByIdRepository<TEntity>, ICommandRepository<TEntity> where TEntity : BaseWriteEntity
{
    public CommandRepository(ApplicationWriteDbContext context) : base(context)
    {
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Context.Set<TEntity>().AddAsync(entity);
        return entity;
    }

    public void Update(TEntity entity, CancellationToken cancellationToken = default)
    {
        Context.Entry(entity).CurrentValues.SetValues(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        TEntity entity = await GetByIdAsync(id);
        entity.Delete();
    }
}