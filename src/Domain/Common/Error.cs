namespace Domain.Common;

/// <summary>
/// Represents an error with a code and description.
/// </summary>
public sealed record Error(string code, string description)
{
    /// <summary>
    /// Represents an empty error.
    /// </summary>
    public static readonly Error None = new Error(string.Empty, string.Empty);

    /// <summary>
    /// Implicitly converts an <see cref="Error"/> to a <see cref="Result"/> by wrapping it in a failure result.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    public static implicit operator Result(Error error) => Result.Failure(error);
}

/// <summary>
/// Represents an error with a code and description.
/// </summary>
/// <typeparam name="T">The type of the value associated with the error.</typeparam>
/// <remarks>
/// This type is useful when you want to associate an error with a value.
/// </remarks>
public sealed record Error<T>(string code, string description)
{
    /// <summary>
    /// Represents an empty error.
    /// </summary>
    public static readonly Error<T> None = new Error<T>(string.Empty, string.Empty);

    /// <summary>
    /// Implicitly converts an <see cref="Error{T}"/> to a <see cref="Result{T}"/> by wrapping it in a failure result.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>A new instance of <see cref="Result{T}"/> representing a failed operation.</returns>
    public static implicit operator Result<T>(Error<T> error)  {
        List<Error<T>> errors = new List<Error<T>> { error };
        return Result<T>.Failure(errors);
    }
}