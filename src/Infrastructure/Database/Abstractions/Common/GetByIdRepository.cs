using System;
using Domain.Abstractions;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Abstractions.Common;

public abstract class GetByIdRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly DbContext Context = default!;

    public GetByIdRepository(DbContext context)
    {
        Context = context;
    }

    public async Task<Result<TEntity>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        TEntity? result = await Context.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (result is null)
        {
            List<Error> errors = new List<Error> { GenericErrors.NotFound(id, typeof(TEntity)) };
            return Result<TEntity>.Failure(errors);
        }

        return Result<TEntity>.Success(result);
    }
}