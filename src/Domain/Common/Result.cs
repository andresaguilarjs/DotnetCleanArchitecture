namespace Domain.Common;

public class Result
{
    public bool IsSuccess { get; }
    public string Error { get; }

    private Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new Result(true, string.Empty);
    public static Result Failure(string error) => new Result(false, error);
}

public class Result<TResult>
{
    public bool IsSuccess { get; }
    public string Error { get; }
    public TResult Value { get; }

    private Result(bool isSuccess, string error, TResult value)
    {
        IsSuccess = isSuccess;
        Error = error;
        Value = value;
    }

    public static Result<TResult> Success(TResult value) => new Result<TResult>(true, string.Empty, value);
    public static Result<TResult> Failure(string error) => new Result<TResult>(false, error, default);
}