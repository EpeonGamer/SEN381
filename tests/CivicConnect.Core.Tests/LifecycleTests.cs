using CivicConnect.Core.Lifecycle;
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
        var request = Staff(RequestStatus.Assigned, assignee: "staff-2") with { ActorRole = role };
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
        new(Id, to, "staff-1", ActorRole.Staff, assignee, reason, note);

    private sealed class FakeRepository : IRequestRepository
    {
        private readonly RequestStatus? _status;
        public FakeRepository(RequestStatus? status) => _status = status;
        public List<TransitionRecord> Saved { get; } = new();
        public bool SaveResult { get; set; } = true;
        public bool ThrowOnSave { get; set; }

        public Task<RequestStatus?> GetStatusAsync(Guid requestId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_status);

        public Task<bool> SaveTransitionAsync(TransitionRecord record, CancellationToken cancellationToken = default)
        {
            if (ThrowOnSave) throw new InvalidOperationException("simulated storage failure");
            if (SaveResult) Saved.Add(record);
            return Task.FromResult(SaveResult);
        }
    }
}
