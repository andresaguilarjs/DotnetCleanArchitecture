using Domain.Common;
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
    /// UsersSeeding method to seed the database with some mock data.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <returns></returns>
    public static async Task UsersSeeding(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
    {
        UserQueryRepository userQueryRepository = new(dbContext);
        UserCommandRepository userCommandRepository = new(dbContext, userQueryRepository);

        for (int i = 0; i < 10; i++)
        {
            Result<Email> email = Email.Create($"mock-{i}@gmail.com");
            Result<FirstName> firstName = FirstName.Create($"Mock {i}");
            Result<LastName> lastName = LastName.Create($"User {i}");

            ValidateUserValueObjects(email, firstName, lastName, i);

            UserEntity user = UserEntity.Create(email, firstName, lastName);
            Result<UserEntity> addResult = await userCommandRepository.AddAsync(user, cancellationToken);

            if (addResult.IsFailure)
            {
                string errorMessages = string.Join("; ", addResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new InvalidOperationException($"Failed to add user {i} to repository: {errorMessages}");
            }
        }
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Validate User Value Objects method to validate the value objects
    /// This method throws an exception if any of the value objects are invalid
    /// This is mean to be used only in the seeding process
    /// </summary>
    /// <param name="emailResult">Email result object</param>
    /// <param name="firstNameResult">FirstName result object</param>
    /// <param name="lastNameResult">LastName result object</param>
    /// <param name="iteration">Current iteration in the for to create the users</param>
    /// <exception cref="InvalidOperationException"></exception>
    private static void ValidateUserValueObjects(Result<Email> emailResult, Result<FirstName> firstNameResult, Result<LastName> lastNameResult, int iteration)
    {
        if (emailResult.IsFailure || firstNameResult.IsFailure || lastNameResult.IsFailure)
        {
            List<string> validationErrors = new();
            if (emailResult.IsFailure) validationErrors.AddRange(emailResult.Errors.Select(e => $"Email: {e.Description}"));
            if (firstNameResult.IsFailure) validationErrors.AddRange(firstNameResult.Errors.Select(e => $"FirstName: {e.Description}"));
            if (lastNameResult.IsFailure) validationErrors.AddRange(lastNameResult.Errors.Select(e => $"LastName: {e.Description}"));

            throw new InvalidOperationException($"Invalid value objects for user {iteration}: {string.Join("; ", validationErrors)}");
        }
    }
}