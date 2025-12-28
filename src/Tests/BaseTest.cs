using Domain.Common;
using Domain.Interfaces;
using Infrastructure.Database.DBContext;
using Moq;

namespace Tests;

/// <summary>
/// Base test class that provides common test infrastructure.
/// Implements IAsyncLifetime for proper async initialization and cleanup.
/// </summary>
public abstract class BaseTest : IAsyncLifetime, IDisposable
{
    protected readonly ApplicationDbContext _dbContext;
    protected readonly Mock<IUnitOfWork> _unitOfWorkMock;

    /// <summary>
    /// BaseTest constructor to create a new instance of the ApplicationDbContext
    /// and a new instance of the Mock IUnitOfWork
    /// </summary>
    public BaseTest()
    {
        _dbContext = InMemoryDatabase.GetDbContext();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock
           .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
           .ReturnsAsync(Result.Success());
    }

    /// <summary>
    /// InitializeAsync is called after the constructor and before any test methods.
    /// Override this method in derived classes to perform async initialization (e.g., seeding).
    /// </summary>
    public virtual Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// DisposeAsync is called after all test methods have completed.
    /// Override this method in derived classes to perform async cleanup.
    /// </summary>
    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Dispose method to delete the database and dispose the ApplicationDbContext
    /// </summary>
    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}