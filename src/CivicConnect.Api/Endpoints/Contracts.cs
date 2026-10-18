using CivicConnect.Core.Lifecycle;
using CivicConnect.Core.Requests;

namespace CivicConnect.Api.Endpoints;

// Request bodies. Properties are nullable so missing fields reach our own validation (clear 400s)
// instead of failing inside the JSON binder.

public sealed record LoginBody(string? UserId, string? Password);

public sealed record SubmitRequestBody(string? Category, string? Title, string? Description);

public sealed record TransitionBody(RequestStatus? To, string? AssigneeId, string? Reason, string? ResolutionNote);

public sealed record CommentBody(string? Content);

public sealed record ReassignBody(string? AssigneeId, long? ExpectedVersion = null);

// Response bodies.

public sealed record UserView(string Id, string Role);

public sealed record RequestSummary(
    Guid Id, string Category, string Title, RequestStatus Status, string RequesterId,
    string? AssigneeId, DateTimeOffset SubmittedAt, DateTimeOffset UpdatedAt,
    DateTimeOffset? DueAt = null, bool IsOverdue = false, long Version = 0)
{
    public static RequestSummary From(ServiceRequest r, DateTimeOffset? now = null) =>
        new(r.Id, r.Category, r.Title, r.Status, r.RequesterId, r.AssigneeId, r.SubmittedAt, r.UpdatedAt,
            OverduePolicy.DueAt(r), OverduePolicy.IsOverdue(r, now ?? DateTimeOffset.UtcNow), r.Version);
}

/// <summary>Full request detail: current status plus its history, so requesters see reasons and notes (FR-007).</summary>
public sealed record RequestDetail(
    RequestSummary Request, string Description,
    IReadOnlyList<TransitionRecord> History, IReadOnlyList<RequestComment> Comments);

public sealed record ActivityCountsView(long Open, long Rejected, long Resolved, long Overdue = 0);
