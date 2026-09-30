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

        // Authorization is owned by ADR-05, not by the lifecycle service.
        if (string.IsNullOrWhiteSpace(request.Actor.Id) ||
            !_visibilityPolicy.GetScope(request.Actor).Allows(AuthorizationAction.ChangeStatus))
            return TransitionResult.Fail(TransitionFailure.NotAuthorised);

        var current = await _repository.GetStatusAsync(request.RequestId, cancellationToken);
        if (current is null)
            return TransitionResult.Fail(TransitionFailure.RequestNotFound);

        if (!TransitionRules.TryGetRequirement(current.Value, request.To, out var required))
            return TransitionResult.Fail(TransitionFailure.InvalidTransition);

        if (required.HasFlag(TransitionRequirement.Assignee) && string.IsNullOrWhiteSpace(request.AssigneeId))
            return TransitionResult.Fail(TransitionFailure.MissingAssignee);
        if (required.HasFlag(TransitionRequirement.Reason) && string.IsNullOrWhiteSpace(request.Reason))
            return TransitionResult.Fail(TransitionFailure.MissingReason);
        if (required.HasFlag(TransitionRequirement.ResolutionNote) && string.IsNullOrWhiteSpace(request.ResolutionNote))
            return TransitionResult.Fail(TransitionFailure.MissingResolutionNote);

        var record = new TransitionRecord(
            request.RequestId, current.Value, request.To, request.Actor.Id, _clock.GetUtcNow(),
            request.AssigneeId, request.Reason, request.ResolutionNote);

        // Storage failures propagate as exceptions; the repository has rolled back (status unchanged).
        var saved = await _repository.SaveTransitionAsync(record, cancellationToken);

        return saved
            ? TransitionResult.Success(request.To)
            : TransitionResult.Fail(TransitionFailure.ConcurrencyConflict);
    }
}
