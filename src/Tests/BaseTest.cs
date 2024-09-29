using Domain.Interfaces;
using Infrastructure.Database.DBContext;
using Moq;

namespace Tests;

public abstract class BaseTest : IDisposable
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