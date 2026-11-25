namespace IncidentAgent.BlazorUi.Services;

/// <summary>Result of an API call: data, or a user-facing error message.</summary>
public sealed record ApiResult<T>(T? Data, string? Error, bool IsCancelled = false, bool IsNotFound = false)
{
    public bool IsSuccess => Error is null && !IsCancelled;

    public static ApiResult<T> Ok(T data) => new(data, null);
    public static ApiResult<T> Fail(string error, bool notFound = false) => new(default, error, IsNotFound: notFound);
    public static ApiResult<T> Cancelled() => new(default, null, IsCancelled: true);
}
