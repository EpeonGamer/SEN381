using CivicConnect.Core.Authorization;
using CivicConnect.Core.Lifecycle;

namespace CivicConnect.Core.Requests;

/// <summary>Core submission, scoped reads, comments and responsibility changes. Actors are reloaded from trusted storage.</summary>
public sealed class RequestService
{
    private readonly IRequestRepository _repository;
    private readonly IVisibilityPolicy _policy;
    private readonly TimeProvider _clock;

    public RequestService(IRequestRepository repository, IVisibilityPolicy? policy = null, TimeProvider? clock = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _policy = policy ?? new RoleVisibilityPolicy();
        _clock = clock ?? TimeProvider.System;
    }

    public async Task<RequestResult> SubmitAsync(ActorContext actor, string category, string title, string description,
        CancellationToken cancellationToken = default)
    {
        var trusted = await GetActorAsync(actor, cancellationToken);
        if (trusted?.Role != ActorRole.Requester)
            return RequestResult.Fail(RequestFailure.NotAuthorised);
        if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
            return RequestResult.Fail(RequestFailure.InvalidInput);
        if (!await _repository.CategoryExistsAsync(category.Trim(), cancellationToken))
            return RequestResult.Fail(RequestFailure.InvalidCategory);
        var now = _clock.GetUtcNow();
        var request = new ServiceRequest(Guid.NewGuid(), trusted.Id, category.Trim(), title.Trim(), description.Trim(),
            RequestStatus.Unassigned, now, now);
        await _repository.CreateAsync(request, cancellationToken);
        return RequestResult.Success(request.Id);
    }

    public async Task<ServiceRequest?> GetAsync(ActorContext actor, Guid id, CancellationToken cancellationToken = default)
    {
        var scope = await GetScopeAsync(actor, cancellationToken);
        // Filtering happens in SQL, before returning any request data.
        return (await _repository.ListAsync(scope, new RequestQuery(Id: id, Limit: 1), cancellationToken)).SingleOrDefault();
    }

    public async Task<IReadOnlyList<ServiceRequest>> ListAsync(ActorContext actor, RequestQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        query ??= new RequestQuery();
        ValidateQuery(query);
        return await _repository.ListAsync(await GetScopeAsync(actor, cancellationToken), query, cancellationToken);
    }

    public async Task<IReadOnlyList<TransitionRecord>> GetHistoryAsync(ActorContext actor, Guid id, CancellationToken cancellationToken = default)
    {
        if (await GetAsync(actor, id, cancellationToken) is null) throw new UnauthorizedAccessException();
        return await _repository.GetHistoryAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<RequestComment>> GetCommentsAsync(ActorContext actor, Guid id, CancellationToken cancellationToken = default)
    {
        if (await GetAsync(actor, id, cancellationToken) is null) throw new UnauthorizedAccessException();
        return await _repository.GetCommentsAsync(id, cancellationToken);
    }

    public async Task<RequestResult> AddCommentAsync(ActorContext actor, Guid id, string content, CancellationToken cancellationToken = default)
    {
        var trusted = await GetActorAsync(actor, cancellationToken);
        if (trusted is null || !_policy.GetScope(trusted).Allows(AuthorizationAction.ChangeStatus))
            return RequestResult.Fail(RequestFailure.NotAuthorised);
        if (await GetAsync(trusted, id, cancellationToken) is null)
            return RequestResult.Fail(RequestFailure.NotAuthorised);
        if (string.IsNullOrWhiteSpace(content)) return RequestResult.Fail(RequestFailure.InvalidInput);
        await _repository.AddCommentAsync(new RequestComment(id, trusted.Id, content.Trim(), _clock.GetUtcNow()), cancellationToken);
        return RequestResult.Success(id);
    }

    public async Task<RequestResult> ReassignAsync(ActorContext actor, Guid id, string assigneeId, CancellationToken cancellationToken = default)
    {
        var trusted = await GetActorAsync(actor, cancellationToken);
        if (trusted is null || !_policy.GetScope(trusted).Allows(AuthorizationAction.ChangeStatus))
            return RequestResult.Fail(RequestFailure.NotAuthorised);
        var request = await GetAsync(trusted, id, cancellationToken);
        if (request is null) return RequestResult.Fail(RequestFailure.NotAuthorised);
        if (request.Status != RequestStatus.Assigned) return RequestResult.Fail(RequestFailure.InvalidState);
        var assignee = string.IsNullOrWhiteSpace(assigneeId) ? null : await _repository.GetActorAsync(assigneeId, cancellationToken);
        if (assignee is null || !_policy.GetScope(assignee).Allows(AuthorizationAction.ChangeStatus) ||
            !_policy.GetScope(assignee).CanView(new RequestAccess(id, request.RequesterId, request.Category)))
            return RequestResult.Fail(RequestFailure.InvalidAssignee);
        if (request.AssigneeId == assigneeId) return RequestResult.Fail(RequestFailure.InvalidInput);
        return await _repository.ReassignAsync(request, assigneeId, trusted.Id, _clock.GetUtcNow(), cancellationToken)
            ? RequestResult.Success(id) : RequestResult.Fail(RequestFailure.ConcurrencyConflict);
    }

    public async Task<ActivityCounts> GetCountsAsync(ActorContext actor, RequestQuery? query = null, CancellationToken cancellationToken = default)
    {
        var trusted = await GetActorAsync(actor, cancellationToken);
        if (trusted?.Role != ActorRole.Manager) throw new UnauthorizedAccessException();
        query ??= new RequestQuery();
        ValidateQuery(query);
        var scope = _policy.GetScope(trusted);
        if (!scope.Allows(AuthorizationAction.ViewRequest)) throw new UnauthorizedAccessException();
        return await _repository.GetCountsAsync(scope, query, cancellationToken);
    }

    private async Task<ActorContext?> GetActorAsync(ActorContext actor, CancellationToken cancellationToken) =>
        actor is null || string.IsNullOrWhiteSpace(actor.Id) ? null : await _repository.GetActorAsync(actor.Id, cancellationToken);

    private async Task<VisibilityScope> GetScopeAsync(ActorContext actor, CancellationToken cancellationToken)
    {
        var trusted = await GetActorAsync(actor, cancellationToken) ?? throw new UnauthorizedAccessException();
        var scope = _policy.GetScope(trusted);
        if (!scope.Allows(AuthorizationAction.ViewRequest)) throw new UnauthorizedAccessException();
        return scope;
    }

    private static void ValidateQuery(RequestQuery query)
    {
        if (query.Limit is < 1 or > 100 || query.Offset < 0 || (query.Status.HasValue && !Enum.IsDefined(query.Status.Value)))
            throw new ArgumentOutOfRangeException(nameof(query));
    }
}
