using CivicConnect.Core.Authorization;

namespace CivicConnect.Core.Lifecycle;

/// <summary>A request to change a service request's status.</summary>
public sealed record TransitionRequest(
    Guid RequestId,
    RequestStatus To,
    ActorContext Actor,
    string? AssigneeId = null,
    string? Reason = null,
    string? ResolutionNote = null);

public enum TransitionFailure
{
    None,
    NotAuthorised,
    RequestNotFound,
    InvalidTransition,
    MissingAssignee,
    MissingReason,
    MissingResolutionNote,
    ConcurrencyConflict
}

public sealed record TransitionResult(bool Succeeded, TransitionFailure Failure, RequestStatus? NewStatus = null)
{
    public static TransitionResult Success(RequestStatus newStatus) => new(true, TransitionFailure.None, newStatus);
    public static TransitionResult Fail(TransitionFailure failure) => new(false, failure);
}

/// <summary>What is persisted for an accepted transition: the new status plus its audit entry.</summary>
public sealed record TransitionRecord(
    Guid RequestId,
    RequestStatus From,
    RequestStatus To,
    string ActorId,
    DateTimeOffset At,
    string? AssigneeId,
    string? Reason,
    string? ResolutionNote);
