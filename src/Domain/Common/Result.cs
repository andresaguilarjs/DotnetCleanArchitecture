namespace Domain.Common
{
    /// <summary>
    /// Represents the result of an operation that can either succeed or fail.
    /// </summary>
    public sealed class Result
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets a value indicating whether the operation failed.
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Gets the error associated with the result, if the operation failed.
        /// </summary>
        public Error Error { get; }

        private Result(bool isSuccess, Error error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        /// <summary>
        /// Creates a new successful result.
        /// </summary>
        /// <returns>A new instance of <see cref="Result"/> representing a successful operation.</returns>
        public static Result Success() => new Result(true, Error.None);

        /// <summary>
        /// Creates a new failed result with the specified error.
        /// </summary>
        /// <param name="error">The error associated with the failed operation.</param>
        /// <returns>A new instance of <see cref="Result"/> representing a failed operation.</returns>
        public static Result Failure(Error error) => new Result(false, error);
    }

    /// <summary>
    /// Represents the result of an operation that can either succeed or fail, with a value.
    /// </summary>
    /// <typeparam name="TResult">The type of the value associated with the result.</typeparam>
    public sealed class Result<TResult>
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets a value indicating whether the operation failed.
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Gets the error associated with the result, if the operation failed.
        /// </summary>
        public IList<Error> Errors { get; }

        /// <summary>
        /// Gets the value associated with the result, if the operation succeeded.
        /// </summary>
        public TResult Value { get; }

        private Result(bool isSuccess, IList<Error> errors, TResult value)
        {
            IsSuccess = isSuccess;
            Errors = errors;
            Value = value;
        }

        /// <summary>
        /// Creates a new successful result with the specified value.
        /// </summary>
        /// <param name="value">The value associated with the successful operation.</param>
        /// <returns>A new instance of <see cref="Result{TResult}"/> representing a successful operation.</returns>
        public static Result<TResult> Success(TResult value) => new Result<TResult>(true, new List<Error>(), value);

        /// <summary>
        /// Creates a new failed result with the specified error.
        /// </summary>
        /// <param name="errors">A list of errors associated with the failed operation.</param>
        /// <returns>A new instance of <see cref="Result{TResult}"/> representing a failed operation.</returns>
        public static Result<TResult> Failure(IList<Error> errors) => new Result<TResult>(false, errors, default!);

        /// <summary>
        /// Implicitly converts a <see cref="Result{TResult}"/> to its value.
        /// </summary>
        /// <param name="result"></param>
        public static implicit operator TResult(Result<TResult> result) => result.Value;
    }
}