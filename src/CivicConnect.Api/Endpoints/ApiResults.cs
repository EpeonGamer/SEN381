using CivicConnect.Core.Lifecycle;
using CivicConnect.Core.Requests;

namespace CivicConnect.Api.Endpoints;

/// <summary>Error body returned by every failing API call: a stable code plus an optional explanation.</summary>
public sealed record ApiError(string Error, string? Detail = null);

/// <summary>
/// Single place where Core results become HTTP status codes, so every endpoint answers the same way.
/// </summary>
public static class ApiResults
{
    public static IResult Error(int statusCode, string error, string? detail = null) =>
        Results.Json(new ApiError(error, detail), statusCode: statusCode);

    public static IResult From(TransitionFailure failure) => failure switch
    {
        TransitionFailure.NotAuthorised => Error(StatusCodes.Status403Forbidden, failure.ToString()),
        TransitionFailure.RequestNotFound => Error(StatusCodes.Status404NotFound, failure.ToString()),
        TransitionFailure.InvalidTransition => Error(StatusCodes.Status409Conflict, failure.ToString(),
            "That status change is not allowed from the request's current status."),
        TransitionFailure.ConcurrencyConflict => Error(StatusCodes.Status409Conflict, failure.ToString(),
            "The request was changed by someone else. Reload it and try again."),
        _ => Error(StatusCodes.Status400BadRequest, failure.ToString()) // missing reason/note/assignee, invalid assignee
    };

    public static IResult From(RequestFailure failure) => failure switch
    {
        RequestFailure.NotAuthorised => Error(StatusCodes.Status403Forbidden, failure.ToString()),
        RequestFailure.InvalidState or RequestFailure.ConcurrencyConflict =>
            Error(StatusCodes.Status409Conflict, failure.ToString()),
        _ => Error(StatusCodes.Status400BadRequest, failure.ToString()) // invalid input/category/assignee
    };
}
