using Domain.Common;
using Domain.Entities.User.ValueObjects;

namespace Domain.Entities.User.Interfaces;

public interface IUserService
{
    /// <summary>
    /// Create a new user entity after validating the user value objects.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="firstName"></param>
    /// <param name="lastName"></param>
    /// <returns></returns>
    Task<Result<UserEntity>> CreateUserEntityAsync(string email, string firstName, string lastName);

    /// <summary>
    /// Update a user entity after validating the user value objects.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="email"></param>
    /// <param name="firstName"></param>
    /// <param name="lastName"></param>
    /// <returns></returns>
    Task<Result<UserEntity>> UpdateUserEntityAsync(UserEntity user, string email, string firstName, string lastName);

    /// <summary>
    /// Create the user value objects.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="firstName"></param>
    /// <param name="lastName"></param>
    /// <returns></returns>
    (Result<Email>, Result<FirstName>, Result<LastName>) CreateUserValueObjects(string email, string firstName, string lastName);

    /// <summary>
    /// Validate the user value objects using their result objects.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="firstName"></param>
    /// <param name="lastName"></param>
    /// <returns></returns>
    IList<Error> ValidateUserValueObjects(Result<Email> email, Result<FirstName> firstName, Result<LastName> lastName);
}