using Domain.Interfaces;
using Infrastructure.Database.DBContext;
using Moq;

namespace Tests;

public abstract class BaseTest : IDisposable
{
    protected readonly InMemoryDatabase _inMemoryDatabase = new();
    protected readonly ApplicationWriteDbContext _writeDbContext;
    protected readonly ApplicationReadDbContext _readDbContext;
    protected readonly Mock<IUnitOfWork> _unitOfWorkMock;

    /// <summary>
    /// BaseTest constructor to create a new instance of the ApplicationDbContext
    /// and a new instance of the Mock IUnitOfWork
    /// </summary>
    public BaseTest()
    {
        _writeDbContext = _inMemoryDatabase.applicationWriteDbContext;
        _readDbContext = _inMemoryDatabase.applicationReadDbContext;
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    /// <summary>
    /// Dispose method to delete the database and dispose the ApplicationDbContext
    /// </summary>
    public void Dispose()
    {
        _readDbContext.Dispose();
        _writeDbContext.Database.EnsureDeleted();
        _writeDbContext.Dispose();
    }
}