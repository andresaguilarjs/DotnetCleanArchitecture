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
public class InMemoryDatabase
{

    public ApplicationWriteDbContext applicationWriteDbContext;
    public ApplicationReadDbContext applicationReadDbContext;

    public InMemoryDatabase()
    {
        string databaseName = Guid.NewGuid().ToString();

        var optionsWrite = new DbContextOptionsBuilder<ApplicationWriteDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        applicationWriteDbContext = new ApplicationWriteDbContext(optionsWrite);
        applicationWriteDbContext.Database.EnsureCreated();

        var optionsRead = new DbContextOptionsBuilder<ApplicationReadDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        applicationReadDbContext = new ApplicationReadDbContext(optionsRead);
    }

    /// <summary>
    /// UserSeeding method to seed the database with some mock data.
    /// </summary>
    /// <returns></returns>
    public async Task UserSeeding()
    {
        UserCommandRepository userCommandRepository = new(applicationWriteDbContext);

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
        applicationWriteDbContext.SaveChanges();
    }

}