using Domain.Common;

namespace Application.Abstractions.Validation;

/// <summary>
/// Represents a validator for request objects.
/// Validators are used in the pipeline behavior to validate input before it reaches the handler.
/// This provides early validation feedback and prevents invalid data from entering the application layer.
/// </summary>
/// <typeparam name="TRequest">The type of request to validate.</typeparam>
public interface IValidator<in TRequest>
{
    /// <summary>
    /// Validates the request and returns a Result indicating success or failure.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result indicating validation success or failure with errors.</returns>
    Task<Result> ValidateAsync(TRequest request, CancellationToken cancellationToken = default);
}