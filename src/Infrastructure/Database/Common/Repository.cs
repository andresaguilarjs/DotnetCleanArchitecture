using Domain.Abstractions;
using Domain.Common;
using Domain.Entities.User;
using Domain.Interfaces;
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
        TEntity currentEntity = await GetByIdAsync(entity.Id);
        Context.Entry(currentEntity).CurrentValues.SetValues(entity);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return currentEntity;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        TEntity entity = await GetByIdAsync(id);
        entity.Delete();
        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<Result<TEntity>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        TEntity? result = await Context.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (result is null)
        {
            List<Error<TEntity>> errors = new List<Error<TEntity>> { UserErrors<TEntity>.NotFound(id) };
            return Result<TEntity>.Failure(errors);
        }

        return Result<TEntity>.Success(result);
    }

    public async Task<IReadOnlyList<TEntity>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<TEntity>()
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken);
    }
}