using CivicConnect.Core.Authorization;

namespace CivicConnect.Core.Lifecycle;

/// <summary>
/// The ONLY code path allowed to change a request's status (ADR-04). UI and API code must call
/// this service and never set status directly.
/// Order of checks: authorisation (FR-001), request exists, transition allowed, required
/// information present, then atomic save with audit entry (NFR-004).
/// </summary>
public sealed class TransitionService
{
    private readonly IRequestRepository _repository;
    private readonly TimeProvider _clock;
    private readonly IVisibilityPolicy _visibilityPolicy;

    public TransitionService(
        IRequestRepository repository,
        IVisibilityPolicy? visibilityPolicy = null,
        TimeProvider? clock = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _visibilityPolicy = visibilityPolicy ?? new RoleVisibilityPolicy();
        _clock = clock ?? TimeProvider.System;
    }

    public async Task<TransitionResult> ApplyAsync(TransitionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Authorization is owned by ADR-05.
        if (request.Actor is null || string.IsNullOrWhiteSpace(request.Actor.Id))
            return TransitionResult.Fail(TransitionFailure.NotAuthorised);

        var actor = await _repository.GetActorAsync(request.Actor.Id, cancellationToken);
        if (actor is null)
            return TransitionResult.Fail(TransitionFailure.NotAuthorised);
        var scope = _visibilityPolicy.GetScope(actor);
        if (!scope.Allows(AuthorizationAction.ChangeStatus))
            return TransitionResult.Fail(TransitionFailure.NotAuthorised);

        var current = await _repository.GetAsync(request.RequestId, cancellationToken);
        if (current is null)
            return TransitionResult.Fail(TransitionFailure.RequestNotFound);

        if (!scope.CanView(new RequestAccess(current.Id, current.RequesterId, current.Category)))
            return TransitionResult.Fail(TransitionFailure.NotAuthorised);

        if (!TransitionRules.TryGetRequirement(current.Status, request.To, out var required))
            return TransitionResult.Fail(TransitionFailure.InvalidTransition);

        // Resolution belongs to the current assignee, regardless of who originally assigned it.
        // The repository's version check also prevents a concurrent reassignment being overwritten.
        if (request.To == RequestStatus.Resolved &&
            !string.Equals(current.AssigneeId, actor.Id, StringComparison.Ordinal))
            return TransitionResult.Fail(TransitionFailure.NotAssigned);

        if (required.HasFlag(TransitionRequirement.Assignee) && string.IsNullOrWhiteSpace(request.AssigneeId))
            return TransitionResult.Fail(TransitionFailure.MissingAssignee);
        if (required.HasFlag(TransitionRequirement.Reason) && string.IsNullOrWhiteSpace(request.Reason))
            return TransitionResult.Fail(TransitionFailure.MissingReason);
        if (required.HasFlag(TransitionRequirement.ResolutionNote) && string.IsNullOrWhiteSpace(request.ResolutionNote))
            return TransitionResult.Fail(TransitionFailure.MissingResolutionNote);

        if (required.HasFlag(TransitionRequirement.Assignee))
        {
            var assignee = await _repository.GetActorAsync(request.AssigneeId!, cancellationToken);
            if (assignee is null || !_visibilityPolicy.GetScope(assignee).Allows(AuthorizationAction.ChangeStatus) ||
                !_visibilityPolicy.GetScope(assignee).CanView(new RequestAccess(current.Id, current.RequesterId, current.Category)))
                return TransitionResult.Fail(TransitionFailure.InvalidAssignee);
        }

        var record = new TransitionRecord(
            request.RequestId, current.Status, request.To, actor.Id, _clock.GetUtcNow(),
            required.HasFlag(TransitionRequirement.Assignee) ? request.AssigneeId : null,
            required.HasFlag(TransitionRequirement.Reason) ? request.Reason!.Trim() : null,
            required.HasFlag(TransitionRequirement.ResolutionNote) ? request.ResolutionNote!.Trim() : null,
            current.Version);

        // Storage failures propagate as exceptions; the repository has rolled back (status unchanged).
        var saved = await _repository.SaveTransitionAsync(record, cancellationToken);

        return saved
            ? TransitionResult.Success(request.To)
            : TransitionResult.Fail(TransitionFailure.ConcurrencyConflict);
    }
}
