using Domain.Interfaces;
using Infrastructure.Database.DBContext;
using Moq;

namespace Tests;

public abstract class BaseTest : IDisposable
{
    protected readonly ApplicationDbContext _dbContext;
    protected readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public BaseTest()
    {
        _dbContext = InMenoryDatabase.GetDbContext();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}