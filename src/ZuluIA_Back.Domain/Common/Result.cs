namespace ZuluIA_Back.Domain.Common;

public class Result
{
    protected Result(bool isSuccess, string? error = null)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException("Un resultado exitoso no puede tener error.");
        if (!isSuccess && string.IsNullOrWhiteSpace(error))
            throw new InvalidOperationException("Un resultado fallido debe tener un error.");

        IsSuccess = isSuccess;
        Error     = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    public static Result Success() => new(true);
    public static Result Failure(string error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(value, true);
    public static Result<T> Failure<T>(string error) => new(default, false, error);
}

public class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T? value, bool isSuccess, string? error = null)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("No se puede acceder al valor de un resultado fallido.");

    public static implicit operator Result<T>(T value) => Success(value);
}
