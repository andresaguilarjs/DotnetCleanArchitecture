using Application.Abstractions.PipelineBehaviors;
using Application.Abstractions.Validation;
using Domain.Common;
using Domain.Interfaces;

namespace Application.Behaviors;

/// <summary>
/// Defines a pipeline behavior that validates incoming requests using one or more validators before passing them to the
/// next handler in the pipeline.
/// </summary>
/// <remarks>If any validator reports validation failures, the pipeline returns a failure response containing all
/// validation errors and does not invoke subsequent handlers. This behavior ensures that only valid requests are
/// processed further in the pipeline.</remarks>
/// <typeparam name="TRequest">The type of the request message to be validated. Must not be null.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the pipeline. Must implement <see cref="IResult{TResponse}"/>.</typeparam>
public class ValidationPipelineBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IResult<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators ?? new List<IValidator<TRequest>>();
    
    /// <summary>
    /// Validates the specified request using all configured validators and, if validation succeeds, invokes the next
    /// handler in the pipeline asynchronously.
    /// </summary>
    /// <remarks>If no validators are configured, the request is passed directly to the next handler without
    /// validation. If any validator fails, the method returns a failure response containing all collected validation
    /// errors and does not invoke the next handler.</remarks>
    /// <param name="request">The request object to be validated and processed. Cannot be null.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <param name="next">A delegate representing the next handler to invoke if validation passes. This function is called asynchronously.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response from the next handler
    /// if validation succeeds, or a failure response containing validation errors if validation fails.</returns>
    public async Task<TResponse> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken,
        Func<Task<TResponse>> next)
    {
        IList<IValidator<TRequest>> validators = _validators as IList<IValidator<TRequest>> ?? _validators.ToList();
        if (validators.Count == 0) return await next();

        List<Error> errors = new();

        foreach (IValidator<TRequest> validator in validators)
        {
            Result validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsFailure)
            {
                errors.AddRange(validationResult.Errors);
            }
        }

        if (errors.Any()) return TResponse.Failure(errors);

        return await next();
    }
}