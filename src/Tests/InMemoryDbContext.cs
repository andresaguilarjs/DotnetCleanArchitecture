using Infrastructure.Database.DBContext;
using Microsoft.EntityFrameworkCore;

namespace Tests;

public static class InMenoryDatabase
{
    public static ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new ApplicationDbContext(options);
        dbContext.Database.EnsureCreated();

        return dbContext;
    }
}