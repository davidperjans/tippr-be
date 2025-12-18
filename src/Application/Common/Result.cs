namespace Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public Error? Error { get; }

    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);

    public static Result Failure(string message, string? code = null)
        => new(false, new Error(ErrorType.Failure, message, code));

    public static Result BusinessRule(string message, string? code = null)
        => new(false, new Error(ErrorType.BusinessRule, message, code));

    public static Result NotFound(string message, string? code = null)
        => new(false, new Error(ErrorType.NotFound, message, code));

    public static Result Unauthorized(string message = "Unauthorized", string? code = null)
        => new(false, new Error(ErrorType.Unauthorized, message, code));

    public static Result Forbidden(string message = "Forbidden", string? code = null)
        => new(false, new Error(ErrorType.Forbidden, message, code));

    public static Result Conflict(string message, string? code = null)
        => new(false, new Error(ErrorType.Conflict, message, code));

    public static Result Validation(Dictionary<string, string[]> errors, string message = "One or more validation errors occurred.", string? code = null)
        => new(false, new Error(ErrorType.Validation, message, code, errors));
}

public sealed class Result<T> : Result
{
    public T? Data { get; }

    private Result(bool isSuccess, T? data, Error? error) : base(isSuccess, error)
    {
        Data = data;
    }

    public static Result<T> Success(T data) => new(true, data, null);

    public static new Result<T> Failure(string message, string? code = null)
        => new(false, default, new Error(ErrorType.Failure, message, code));

    public static Result<T> BusinessRule(string message, string? code = null)
        => new(false, default, new Error(ErrorType.BusinessRule, message, code));

    public static Result<T> NotFound(string message, string? code = null)
        => new(false, default, new Error(ErrorType.NotFound, message, code));

    public static Result<T> Unauthorized(string message = "Unauthorized", string? code = null)
        => new(false, default, new Error(ErrorType.Unauthorized, message, code));

    public static Result<T> Forbidden(string message = "Forbidden", string? code = null)
        => new(false, default, new Error(ErrorType.Forbidden, message, code));

    public static Result<T> Conflict(string message, string? code = null)
        => new(false, default, new Error(ErrorType.Conflict, message, code));

    public static Result<T> Validation(Dictionary<string, string[]> errors, string message = "One or more validation errors occurred.", string? code = null)
        => new(false, default, new Error(ErrorType.Validation, message, code, errors));
}
