using IncidentAgent.Application.Common;

namespace IncidentAgent.Api.Endpoints;

public static class ResultExtensions
{
    /// <summary>Maps a use-case result to an HTTP result. Failures become ProblemDetails.</summary>
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess) => result.Status switch
    {
        ResultStatus.Success => onSuccess(result.Value!),
        ResultStatus.Validation => Results.ValidationProblem(result.ValidationErrors),
        ResultStatus.NotFound => Results.Problem(result.Error, statusCode: StatusCodes.Status404NotFound, title: "Not found"),
        ResultStatus.Conflict => Results.Problem(result.Error, statusCode: StatusCodes.Status409Conflict, title: "Conflict"),
        _ => Results.Problem("Unexpected result.", statusCode: StatusCodes.Status500InternalServerError)
    };
}
