using Neurointegration.Api.Excpetions;

namespace Neurointegration.Api.DataModels.Result;

public record Result<T> : Result
{
    public T Value { get; private set; }

    public static Result<T> Success(T value)
    {
        return new Result<T>()
        {
            Value = value,
            IsSuccess = true
        };
    }

    public new static Result<T> Fail(Error error)
    {
        return new Result<T>()
        {
            Error = error,
            IsSuccess = false
        };
    }
}

public record Result
{
    public Error Error { get; protected set; }
    public bool IsSuccess { get; protected set; }

    public static Result Success()
    {
        return new Result()
        {
            IsSuccess = true
        };
    }

    public static Result Fail(Error error)
    {
        return new Result()
        {
            Error = error,
            IsSuccess = false
        };
    }
}