namespace IncidentAgent.Application.Common;

public enum ResultStatus
{
    Success,
    NotFound,
    Validation,
    Conflict
}

/// <summary>Outcome of a use case. Expected failures are returned, not thrown.</summary>
public sealed class Result<T>
{
    private Result(ResultStatus status, T? value, string? error, IReadOnlyDictionary<string, string[]>? validationErrors)
    {
        Status = status;
        Value = value;
        Error = error;
        ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
    }

    public ResultStatus Status { get; }
    public T? Value { get; }
    public string? Error { get; }
    public IReadOnlyDictionary<string, string[]> ValidationErrors { get; }
    public bool IsSuccess => Status == ResultStatus.Success;

    public static Result<T> Success(T value) => new(ResultStatus.Success, value, null, null);
    public static Result<T> NotFound(string error) => new(ResultStatus.NotFound, default, error, null);
    public static Result<T> Conflict(string error) => new(ResultStatus.Conflict, default, error, null);

    public static Result<T> Validation(IReadOnlyDictionary<string, string[]> errors) =>
        new(ResultStatus.Validation, default, "One or more validation errors occurred.", errors);
}
