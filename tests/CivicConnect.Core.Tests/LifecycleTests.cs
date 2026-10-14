using CivicConnect.Core.Authorization;
using CivicConnect.Core.Lifecycle;
using CivicConnect.Core.Requests;
using Xunit;

namespace CivicConnect.Core.Tests;

/// <summary>Verifies FR-007, FR-009, FR-011, FR-015 and NFR-004 behaviour (ADR-04, CR-M2-01).</summary>
public class LifecycleTests
{
    private static readonly Guid Id = Guid.NewGuid();

    // All 16 (from, to) pairs. Exactly 3 are allowed.
    public static IEnumerable<object[]> AllPairs()
    {
        var statuses = Enum.GetValues<RequestStatus>();
        foreach (var from in statuses)
            foreach (var to in statuses)
                yield return new object[] { from, to };
    }

    [Theory]
    [MemberData(nameof(AllPairs))]
    public void Table_allows_only_the_three_approved_transitions(RequestStatus from, RequestStatus to)
    {
        var expected =
            (from == RequestStatus.Unassigned && to == RequestStatus.Assigned) ||
            (from == RequestStatus.Unassigned && to == RequestStatus.Rejected) ||
            (from == RequestStatus.Assigned && to == RequestStatus.Resolved);

        Assert.Equal(expected, TransitionRules.TryGetRequirement(from, to, out _));
    }

    [Fact]
    public async Task Assign_succeeds_and_saves_once()
    {
        var repo = new FakeRepository(RequestStatus.Unassigned);
        var result = await new TransitionService(repo).ApplyAsync(Staff(RequestStatus.Assigned, assignee: "staff-2"));

        Assert.True(result.Succeeded);
        Assert.Equal(RequestStatus.Assigned, result.NewStatus);
        Assert.Single(repo.Saved);
    }

    [Fact]
    public async Task Reject_without_reason_is_refused_and_nothing_is_saved()
    {
        var repo = new FakeRepository(RequestStatus.Unassigned);
        var result = await new TransitionService(repo).ApplyAsync(Staff(RequestStatus.Rejected, reason: "  "));

        Assert.Equal(TransitionFailure.MissingReason, result.Failure);
        Assert.Empty(repo.Saved);
    }

    [Fact]
    public async Task Resolve_without_note_is_refused()
    {
        var repo = new FakeRepository(RequestStatus.Assigned);
        var result = await new TransitionService(repo).ApplyAsync(Staff(RequestStatus.Resolved));

        Assert.Equal(TransitionFailure.MissingResolutionNote, result.Failure);
        Assert.Empty(repo.Saved);
    }

    [Fact]
    public async Task Cannot_resolve_an_unassigned_request()
    {
        var repo = new FakeRepository(RequestStatus.Unassigned);
        var result = await new TransitionService(repo).ApplyAsync(Staff(RequestStatus.Resolved, note: "done"));

        Assert.Equal(TransitionFailure.InvalidTransition, result.Failure);
        Assert.Empty(repo.Saved);
    }

    [Theory]
    [InlineData(ActorRole.Requester)]
    [InlineData(ActorRole.Manager)]
    public async Task Only_staff_may_change_status(ActorRole role)
    {
        var repo = new FakeRepository(RequestStatus.Unassigned);
        var request = Staff(RequestStatus.Assigned, assignee: "staff-2") with
        {
            Actor = new ActorContext("actor-1", role)
        };
        repo.Actor = request.Actor;
        var result = await new TransitionService(repo).ApplyAsync(request);

        Assert.Equal(TransitionFailure.NotAuthorised, result.Failure);
        Assert.Empty(repo.Saved);
    }

    [Fact]
    public async Task Unknown_request_is_reported()
    {
        var repo = new FakeRepository(null);
        var result = await new TransitionService(repo).ApplyAsync(Staff(RequestStatus.Assigned, assignee: "staff-2"));

        Assert.Equal(TransitionFailure.RequestNotFound, result.Failure);
    }

    [Fact]
    public async Task Concurrent_change_is_reported_as_conflict()
    {
        var repo = new FakeRepository(RequestStatus.Unassigned) { SaveResult = false };
        var result = await new TransitionService(repo).ApplyAsync(Staff(RequestStatus.Assigned, assignee: "staff-2"));

        Assert.Equal(TransitionFailure.ConcurrencyConflict, result.Failure);
    }

    [Fact]
    public async Task Storage_failure_propagates_and_is_not_reported_as_success()
    {
        var repo = new FakeRepository(RequestStatus.Unassigned) { ThrowOnSave = true };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new TransitionService(repo).ApplyAsync(Staff(RequestStatus.Assigned, assignee: "staff-2")));
    }

    private static TransitionRequest Staff(RequestStatus to, string? assignee = null, string? reason = null, string? note = null) =>
        new(Id, to, new ActorContext("staff-1", ActorRole.Staff,
            new[] { new StaffGroup("General", new HashSet<string> { "General" }) }),
            assignee, reason, note);

    [Fact]
    public async Task Staff_outside_request_category_cannot_change_status()
    {
        var repo = new FakeRepository(RequestStatus.Unassigned)
        {
            Actor = new("staff-1", ActorRole.Staff, new[] { new StaffGroup("Roads", new HashSet<string> { "Road" }) })
        };
        var result = await new TransitionService(repo).ApplyAsync(Staff(RequestStatus.Assigned, assignee: "staff-2"));
        Assert.Equal(TransitionFailure.NotAuthorised, result.Failure);
        Assert.Empty(repo.Saved);
    }

    [Theory]
    [InlineData(RequestStatus.Rejected)]
    [InlineData(RequestStatus.Resolved)]
    public async Task Terminal_requests_cannot_be_assigned_again(RequestStatus terminal)
    {
        var repo = new FakeRepository(terminal);
        var result = await new TransitionService(repo).ApplyAsync(Staff(RequestStatus.Assigned, assignee: "staff-2"));
        Assert.Equal(TransitionFailure.InvalidTransition, result.Failure);
        Assert.Empty(repo.Saved);
    }

    [Fact]
    public async Task Null_actor_is_denied_without_saving()
    {
        var repo = new FakeRepository(RequestStatus.Unassigned);
        var result = await new TransitionService(repo).ApplyAsync(Staff(RequestStatus.Assigned, assignee: "staff-2") with { Actor = null! });
        Assert.Equal(TransitionFailure.NotAuthorised, result.Failure);
        Assert.Empty(repo.Saved);
    }

    [Fact]
    public async Task Rejection_audit_records_server_time_actor_and_only_relevant_information()
    {
        var repo = new FakeRepository(RequestStatus.Unassigned);
        var at = new DateTimeOffset(2026, 10, 14, 0, 0, 0, TimeSpan.Zero);
        var result = await new TransitionService(repo, clock: new FixedClock(at)).ApplyAsync(
            Staff(RequestStatus.Rejected, assignee: "irrelevant", reason: " Duplicate ", note: "irrelevant"));
        Assert.True(result.Succeeded);
        var record = Assert.Single(repo.Saved);
        Assert.Equal(Id, record.RequestId);
        Assert.Equal("staff-1", record.ActorId);
        Assert.Equal(at, record.At);
        Assert.Equal("Duplicate", record.Reason);
        Assert.Null(record.AssigneeId);
        Assert.Null(record.ResolutionNote);
    }

    private sealed class FixedClock(DateTimeOffset at) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => at;
    }

    private sealed class FakeRepository : IRequestRepository
    {
        private readonly RequestStatus? _status;
        public FakeRepository(RequestStatus? status) => _status = status;
        public List<TransitionRecord> Saved { get; } = new();
        public bool SaveResult { get; set; } = true;
        public bool ThrowOnSave { get; set; }
        public ActorContext Actor { get; set; } = new("staff-1", ActorRole.Staff,
            new[] { new StaffGroup("General", new HashSet<string> { "General" }) });

        public Task<ServiceRequest?> GetAsync(Guid requestId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_status is null ? null : new ServiceRequest(requestId, "requester-1", "General", "Title", "Description",
                _status.Value, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow));

        public Task<ActorContext?> GetActorAsync(string actorId, CancellationToken cancellationToken = default) =>
            Task.FromResult<ActorContext?>(actorId == Actor.Id ? Actor : Actor with { Id = actorId, Role = ActorRole.Staff });

        public Task<bool> CategoryExistsAsync(string category, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task CreateAsync(ServiceRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<ServiceRequest>> ListAsync(VisibilityScope scope, RequestQuery query, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<TransitionRecord>> GetHistoryAsync(Guid requestId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<RequestComment>> GetCommentsAsync(Guid requestId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task AddCommentAsync(RequestComment comment, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> ReassignAsync(ServiceRequest expected, string assigneeId, string actorId, DateTimeOffset at, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<ActivityCounts> GetCountsAsync(VisibilityScope scope, RequestQuery query, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<bool> SaveTransitionAsync(TransitionRecord record, CancellationToken cancellationToken = default)
        {
            if (ThrowOnSave) throw new InvalidOperationException("simulated storage failure");
            if (SaveResult) Saved.Add(record);
            return Task.FromResult(SaveResult);
        }
    }
}
