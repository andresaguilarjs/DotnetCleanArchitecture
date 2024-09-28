using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using Domain.Entities.User.ValueObjects;

namespace Application.Users.Services;

/// <summary>
/// Represents a service for user entities.
/// This class should contain methods that would make easy to work with user entities applying the business rules.
/// </summary>
public sealed class UserService : IUserService
{
    private readonly IUserQueryRepository _userQueryRepository;

    public UserService(IUserQueryRepository userQueryRepository)
    {
        _userQueryRepository = userQueryRepository;
    }

    /// <summary>
    /// Create a new user entity after validating the user value objects.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="firstName"></param>
    /// <param name="lastName"></param>
    /// <returns></returns>
    public Result<UserEntity> CreateUserEntity(string email, string firstName, string lastName)
    {
        (Result<Email> emailValue, Result<FirstName> firstNameValue, Result<LastName> lastNameValue) = CreateUserValueObjects(email, firstName, lastName);

        IList<Error> errors = ValidateUserValueObjects(emailValue, firstNameValue, lastNameValue);

        if (!IsEmailAvailable(emailValue))
        {
            errors.Add(UserErrors.EmailAlreadyInUse(email));
        }

        if (errors.Any())
        {
            return Result<UserEntity>.Failure(errors);
        }

        return Result<UserEntity>.Success(UserEntity.Create(emailValue.Value, firstNameValue.Value, lastNameValue.Value));
    }

    /// <summary>
    /// Update a user entity after validating the user value objects.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="email"></param>
    /// <param name="firstName"></param>
    /// <param name="lastName"></param>
    /// <returns></returns>
    public Result<UserEntity> UpdateUserEntity(UserEntity user, string email, string firstName, string lastName)
    {
        (Result<Email> emailValue, Result<FirstName> firstNameValue, Result<LastName> lastNameValue) = CreateUserValueObjects(email, firstName, lastName);

        IList<Error> errors = ValidateUserValueObjects(emailValue, firstNameValue, lastNameValue);

        if (!CanChangeEmail(user.Email, emailValue))
        {
            errors.Add(UserErrors.EmailAlreadyInUse(emailValue.Value.Value));
        }

        if (errors.Any())
        {
            return Result<UserEntity>.Failure(errors);
        }

        user.Update(emailValue.Value, firstNameValue.Value, lastNameValue.Value);

        return Result<UserEntity>.Success(user);
    }

    /// <summary>
    /// Create the user value objects.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="firstName"></param>
    /// <param name="lastName"></param>
    /// <returns></returns>
    public (Result<Email>, Result<FirstName>, Result<LastName>) CreateUserValueObjects(string email, string firstName, string lastName)
    {
        Result<Email> emailValue = Email.Create(email);
        Result<FirstName> firstNameValue = FirstName.Create(firstName);
        Result<LastName> lastNameValue = LastName.Create(lastName);

        return (emailValue, firstNameValue, lastNameValue);
    }

    /// <summary>
    /// Validate the user value objects using their result objects.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="firstName"></param>
    /// <param name="lastName"></param>
    /// <returns></returns>
    public IList<Error> ValidateUserValueObjects(Result<Email> email, Result<FirstName> firstName, Result<LastName> lastName)
    {
        List<Error> errors = new();

        if (email.IsFailure)
        {
            errors.AddRange(email.Errors);
        }

        if (firstName.IsFailure)
        {
            errors.AddRange(firstName.Errors);
        }

        if (lastName.IsFailure)
        {
            errors.AddRange(lastName.Errors);
        }

        return errors;
    }

    /// <summary>
    /// Validate if the email can be changed.
    /// This will check in the database if the email is already in use by another user and return false if it is.
    /// </summary>
    /// <param name="currentEmail"></param>
    /// <param name="newEmail"></param>
    /// <returns></returns>
    private bool CanChangeEmail(Email currentEmail, Email newEmail)
    {
        if (currentEmail != newEmail)
        {
            return IsEmailAvailable(newEmail);
        }

        return true;
    }

    /// <summary>
    /// Validates if an email address is available to be used
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    private bool IsEmailAvailable(Email email)
    {
        Result<UserEntity> userByEmail = _userQueryRepository.GetByEmailAsync(email).Result;
        return userByEmail.IsFailure;
    }
}