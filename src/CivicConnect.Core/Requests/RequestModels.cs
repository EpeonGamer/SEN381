using CivicConnect.Core.Lifecycle;

namespace CivicConnect.Core.Requests;

public sealed record ServiceRequest(Guid Id, string RequesterId, string Category, string Title,
    string Description, RequestStatus Status, DateTimeOffset SubmittedAt, DateTimeOffset UpdatedAt,
    string? AssigneeId = null, long Version = 0);

public sealed record RequestComment(Guid RequestId, string AuthorId, string Content, DateTimeOffset At);

public sealed record RequestQuery(Guid? Id = null, string? Category = null, RequestStatus? Status = null,
    bool SortByCategory = false, int Limit = 50, int Offset = 0, bool? Overdue = null);

public sealed record ActivityCounts(long Open, long Rejected, long Resolved, long Overdue = 0);

public enum RequestFailure
{
    None, NotAuthorised, InvalidInput, InvalidCategory,
    InvalidAssignee, InvalidState, ConcurrencyConflict
}

public sealed record RequestResult(bool Succeeded, RequestFailure Failure, Guid? RequestId = null)
{
    public static RequestResult Fail(RequestFailure failure) => new(false, failure);
    public static RequestResult Success(Guid id) => new(true, RequestFailure.None, id);
}
