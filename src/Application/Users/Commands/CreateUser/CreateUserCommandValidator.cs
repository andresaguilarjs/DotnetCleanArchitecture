using Application.Abstractions.Validation;
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using Domain.Entities.User.ValueObjects;

namespace Application.Users.Commands.CreateUser;

/// <summary>
/// Validates CreateUserCommand input before it reaches the handler.
/// This provides early validation feedback for input format and basic constraints.
/// 
/// Note: This is complementary to domain object validations. Domain validations
/// (Email.Create, FirstName.Create, etc.) enforce business rules and invariants.
/// This validator focuses on input validation at the application boundary.
/// </summary>
public class CreateUserCommandValidator(
    IUserQueryRepository userQueryRepository
    ) : IValidator<CreateUserCommand>
{
    private readonly IUserQueryRepository _userQueryRepository = userQueryRepository;

    /// <summary>
    /// Asynchronously validates the specified user creation request and returns the result of the validation.
    /// </summary>
    /// <remarks>Validation includes checks for email format and availability, as well as first and last name
    /// requirements. The operation is fully asynchronous and may perform database queries to check email availability.</remarks>
    /// <param name="request">The user creation command containing the user details to validate. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the validation operation.</param>
    /// <returns>A task that represents the asynchronous validation operation. The task result contains a <see cref="Result"/>
    /// indicating success if the request is valid; otherwise, contains validation errors.</returns>
    public async Task<Result> ValidateAsync(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        List<Error> errors = new();
        List<Error> emailErrors = await ValidateEmailAsync(request.UserRequest.Email, cancellationToken);
        errors.AddRange(emailErrors);

        if (string.IsNullOrWhiteSpace(request.UserRequest.FirstName))
        {
            errors.Add(UserErrors.EmptyName("first name"));
        }
        else if (request.UserRequest.FirstName.Length < 2 || request.UserRequest.FirstName.Length > 50)
        {
            errors.Add(UserErrors.InvalidNameLength("first name"));
        }

        if (string.IsNullOrWhiteSpace(request.UserRequest.LastName))
        {
            errors.Add(UserErrors.EmptyName("last name"));
        }
        else if (request.UserRequest.LastName.Length < 2 || request.UserRequest.LastName.Length > 50)
        {
            errors.Add(UserErrors.InvalidNameLength("last name"));
        }

        return errors.Any() 
            ? Result.Failure(errors) 
            : Result.Success();
    }

    /// <summary>
    /// Validates the specified email address and adds any validation errors to the provided error list.
    /// </summary>
    /// <param name="email">The email address to validate. Cannot be null or whitespace.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the validation operation.</param>
    /// <returns>A task that represents the asynchronous validation operation. The task result contains the list of errors, including any new errors related to the email validation.</returns>
    private async Task<List<Error>> ValidateEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        List<Error> errors = new();
        if (string.IsNullOrWhiteSpace(email))
        {
            errors.Add(UserErrors.EmptyEmail());
        }
        else if (!Email.IsValid(email))
        {
            errors.Add(UserErrors.InvalidEmail());
        }

        if (errors.Any())
        {
            return errors;
        }

        Result<Email> emailAddress = Email.Create(email);
        if (emailAddress.IsFailure)
        {
            errors.AddRange(emailAddress.Errors);
            return errors;
        }

        if (!await IsEmailAvailable(emailAddress.Value, cancellationToken))
        {
            errors.Add(UserErrors.EmailAlreadyInUse(email));
        }
        return errors;
    }

    /// <summary>
    /// Determines whether the specified email address is available for registration.
    /// </summary>
    /// <param name="email">The email address to check for availability. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if the email
    /// address is not associated with an existing user; otherwise, <see langword="false"/>.</returns>
    private async Task<bool> IsEmailAvailable(Email email, CancellationToken cancellationToken = default)
    {
        Result<UserEntity> userByEmail = await _userQueryRepository.GetByEmailAsync(email);
        return userByEmail.IsFailure;
    }
}