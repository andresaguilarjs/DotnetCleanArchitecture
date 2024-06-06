namespace Infrastructure.Database.UnitOfWork;

public interface IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken);
}
