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
    protected readonly IUnitOfWork UnitOfWork;

    private readonly IQueryRepository<TEntity> _queryRepository;

    public CommandRepository(ApplicationDbContext context, IUnitOfWork unitOfWork, IQueryRepository<TEntity> queryRepository)
    {
        Context = context;
        UnitOfWork = unitOfWork;
        _queryRepository = queryRepository;
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Context.Set<TEntity>().AddAsync(entity);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Context.Entry(entity).CurrentValues.SetValues(entity);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        TEntity entity = await _queryRepository.GetByIdAsync(id);
        entity.Delete();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}