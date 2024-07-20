namespace Domain.Common;

/// <summary>
/// Represents an error with a code and description.
/// </summary>
public sealed record Error(ErrorCodes Code, string Description)
{
    /// <summary>
    /// Represents an empty error.
    /// </summary>
    public static readonly Error None = new Error(ErrorCodes.Empty, string.Empty);

    /// <summary>
    /// Implicitly converts an <see cref="Error"/> to a <see cref="Result"/> by wrapping it in a failure result.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    public static implicit operator Result(Error error) => Result.Failure(error);
}