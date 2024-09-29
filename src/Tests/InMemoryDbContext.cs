using Domain.Entities.User;
using Domain.Entities.User.ValueObjects;
using Infrastructure.Database.DBContext;
using Infrastructure.Database.Repositories.Command;
using Infrastructure.Database.Repositories.Query;
using Microsoft.EntityFrameworkCore;

namespace Tests;

/// <summary>
/// InMemoryDatabase class to create a new instance of the ApplicationDbContext
/// and seed the database with some mock data.
/// </summary>
internal static class InMemoryDatabase
{
    /// <summary>
    /// GetDbContext method to create a new instance of the ApplicationDbContext
    /// </summary>
    /// <returns></returns>
    public static ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new ApplicationDbContext(options);
        dbContext.Database.EnsureCreated();

        return dbContext;
    }

    /// <summary>
    /// UserSeeding method to seed the database with some mock data.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <returns></returns>
    public static async Task UserSeeding(ApplicationDbContext dbContext)
    {
        UserQueryRepository userQueryRepository = new(dbContext);
        UserCommandRepository userCommandRepository = new(dbContext, userQueryRepository);

        for (int i = 0; i < 10; i++)
        {
            await userCommandRepository.AddAsync(
                UserEntity.Create(
                    Email.Create($"mock-{i}@gmail.com"),
                    FirstName.Create($"Mock {i}"),
                    LastName.Create($"User {i}")
                )
            );
        }
        dbContext.SaveChanges();
    }
}