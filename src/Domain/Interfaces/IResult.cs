using Domain.Common;

namespace Domain.Interfaces;

/// <summary>
/// Defines a result type that provides a static method for creating a failure result with a collection of errors.
/// </summary>
/// <remarks>Implementations of this interface should provide a static <c>Failure</c> method that returns an
/// instance representing a failed result. This pattern enables consistent error handling and result creation across
/// different result types.</remarks>
/// <typeparam name="TSelf">The type that implements the result interface. Must inherit from <see cref="IResult{TSelf}"/>.</typeparam>
public interface IResult<TSelf>
    where TSelf : IResult<TSelf>
{
    static abstract TSelf Failure(IList<Error> errors);
}
