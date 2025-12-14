using Domain.Common;

namespace Domain.Interfaces;

public interface IUnitOfWork
{
    public Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default);
}
