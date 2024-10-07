using Domain.Abstractions;
using Domain.Common;
using Domain.Entities.User;
using Domain.Interfaces;
using Infrastructure.Database.Abstractions.Common;
using Infrastructure.Database.DBContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Abstractions;

public abstract class QueryRepository<TEntity> : GetByIdRepository<TEntity>, IQueryRepository<TEntity> where TEntity : BaseReadEntity
{
    public QueryRepository(ApplicationReadDbContext context) : base(context)
    {
    }

    public async Task<Result<IReadOnlyList<TEntity>>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TEntity> entities = await Context.Set<TEntity>()
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<TEntity>>.Success(entities);
    }
}