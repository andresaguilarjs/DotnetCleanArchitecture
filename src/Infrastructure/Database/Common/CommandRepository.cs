using Domain.Abstractions;
using Domain.Common;
using Domain.Entities.User;
using Domain.Interfaces;
using Infrastructure.Database.DBContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Common;

public class CommandRepository<TEntity> : ICommandRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly ApplicationDbContext Context;
    private readonly IQueryRepository<TEntity> _queryRepository;

    public CommandRepository(ApplicationDbContext context, IQueryRepository<TEntity> queryRepository)
    {
        Context = context;
        _queryRepository = queryRepository;
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
        TEntity entity = await _queryRepository.GetByIdAsync(id);
        entity.Delete();
    }
}