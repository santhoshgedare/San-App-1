namespace IdentityHub.Application.Common.Models;

/// <summary>
/// Lightweight result wrapper so handlers can return typed success/failure without throwing for expected failures.
/// </summary>
public class Result
{
    public bool Succeeded { get; }
    public string[] Errors { get; }

    protected Result(bool succeeded, string[] errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public static Result Success() => new(true, []);
    public static Result Failure(params string[] errors) => new(false, errors);
}

public sealed class Result<T> : Result
{
    public T? Data { get; }

    private Result(bool succeeded, T? data, string[] errors) : base(succeeded, errors)
    {
        Data = data;
    }

    public static Result<T> Success(T data) => new(true, data, []);
    public static new Result<T> Failure(params string[] errors) => new(false, default, errors);
}
