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

    public async Task<Result<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Context.Set<TEntity>().AddAsync(entity);

        if (entity != null)
        {
            return Result<TEntity>.Success(entity);
        }

        return Result<TEntity>.Failure(new List<Error>() { GenericErrors.SomethingWhenWrong() });
    }

    public void Update(TEntity entity, CancellationToken cancellationToken = default)
    {
        Context.Entry(entity).CurrentValues.SetValues(entity);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        TEntity entity = await _queryRepository.GetByIdAsync(id);
        if (entity == null)
        {
            return Result.Failure(GenericErrors.NotFound(id, typeof(TEntity)));
        }

        entity.Delete();
        return Result.Success();
    }
}