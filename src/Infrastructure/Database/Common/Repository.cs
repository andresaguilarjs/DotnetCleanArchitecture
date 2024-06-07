using Domain.Common;

namespace Infrastructure.Database.Common;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly ApplicationDbContext Context;
    public Repository(ApplicationDbContext context)
    {
        Context = context;
    }

    public Task<TEntity> AddAsync(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task<TEntity> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TEntity>> ListAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(TEntity entity)
    {
        throw new NotImplementedException();
    }
}