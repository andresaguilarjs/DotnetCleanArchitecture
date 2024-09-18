using Domain.Abstractions;
using Domain.Common;
using Domain.Entities.User;
using Domain.Interfaces;
using Infrastructure.Database.DBContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Common;

public class QueryRepository<TEntity> : IQueryRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly ApplicationDbContext Context;
    public QueryRepository(ApplicationDbContext context)
    {
        Context = context;
    }

    public async Task<Result<TEntity>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        TEntity? result = await Context.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (result is null)
        {
            List<Error> errors = new List<Error> { GenericErrors.NotFound(id, typeof(TEntity)) };
            return Result<TEntity>.Failure(errors);
        }

        return Result<TEntity>.Success(result);
    }

    public async Task<Result<IReadOnlyList<TEntity>>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TEntity> entities = await Context.Set<TEntity>()
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<TEntity>>.Success(entities);
    }
}