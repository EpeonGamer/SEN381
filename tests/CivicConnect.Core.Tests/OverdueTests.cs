using CivicConnect.Core.Lifecycle;
using CivicConnect.Core.Requests;
using CivicConnect.Data;

namespace CivicConnect.Core.Tests;

/// <summary>FR-013 / CR-M3-02: boundary values, terminal states, filtering and persistence.</summary>
public class OverdueTests
{
    private static readonly DateTimeOffset Submitted = new(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(RequestStatus.Unassigned, -1, false)]
    [InlineData(RequestStatus.Unassigned, 0, false)]
    [InlineData(RequestStatus.Unassigned, 1, true)]
    [InlineData(RequestStatus.Assigned, -1, false)]
    [InlineData(RequestStatus.Assigned, 0, false)]
    [InlineData(RequestStatus.Assigned, 1, true)]
    [InlineData(RequestStatus.Rejected, 1, false)]
    [InlineData(RequestStatus.Resolved, 1, false)]
    public async Task Three_day_boundary_matches_policy_sql_counts_and_filters(RequestStatus status, long ticksAfterDue, bool expected)
    {
        var now = Submitted.AddHours(72).AddTicks(ticksAfterDue);
        using var db = await SqliteTestDatabase.CreateAsync(new TestClock(now));
        var request = new ServiceRequest(Guid.NewGuid(), "req-1", "Roads", "Boundary", "FR-013 boundary",
            status, Submitted, Submitted);
        await db.Repository.CreateAsync(request);

        Assert.Equal(Submitted.AddHours(72), OverduePolicy.DueAt(request));
        Assert.Equal(expected, OverduePolicy.IsOverdue(request, now));
        var counts = await db.Requests.GetCountsAsync(SqliteTestDatabase.Manager);
        Assert.Equal(expected ? 1L : 0L, counts.Overdue);
        var overdue = await db.Requests.ListAsync(SqliteTestDatabase.Manager, new RequestQuery(Overdue: true));
        var notOverdue = await db.Requests.ListAsync(SqliteTestDatabase.Manager, new RequestQuery(Overdue: false));
        Assert.Equal(expected ? 1 : 0, overdue.Count);
        Assert.Equal(expected ? 0 : 1, notOverdue.Count);
    }

    [Fact]
    public async Task Derived_overdue_works_for_existing_rows_and_preserves_scope_filters_and_pagination()
    {
        var clock = new TestClock(Submitted);
        using var db = await SqliteTestDatabase.CreateAsync(clock);
        var roads = await db.SubmitAsync(SqliteTestDatabase.Requester1);
        await db.SubmitAsync(SqliteTestDatabase.Requester2);
        await db.SubmitAsync(SqliteTestDatabase.Requester1, "General");
        clock.Now = Submitted.AddDays(3).AddTicks(1);

        Assert.Single(await db.Requests.ListAsync(SqliteTestDatabase.Requester2, new RequestQuery(Overdue: true)));
        Assert.Equal(2, (await db.Requests.ListAsync(SqliteTestDatabase.RoadsStaff, new RequestQuery(Overdue: true))).Count);
        Assert.Single(await db.Requests.ListAsync(SqliteTestDatabase.GeneralStaff, new RequestQuery(Overdue: true)));
        var page = await db.Requests.ListAsync(SqliteTestDatabase.Manager, new RequestQuery(Limit: 1, Offset: 1, Overdue: true));
        Assert.Single(page);
        var counts = await db.Requests.GetCountsAsync(SqliteTestDatabase.Manager, new RequestQuery(Category: "Roads", Overdue: true));
        Assert.Equal(new ActivityCounts(2, 0, 0, 2), counts);
        Assert.Equal(new ActivityCounts(0, 0, 0, 0), await db.Requests.GetCountsAsync(SqliteTestDatabase.Manager,
            new RequestQuery(Status: RequestStatus.Resolved, Overdue: true)));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => db.Requests.GetCountsAsync(SqliteTestDatabase.Requester1));

        // A new repository reading the same existing file needs no migration or stored overdue flag.
        var reopened = new SqliteRequestRepository(db.ConnectionString, clock);
        var existing = await reopened.GetAsync(roads);
        Assert.True(OverduePolicy.IsOverdue(existing!, clock.Now));
        Assert.Equal(RequestStatus.Unassigned, existing!.Status);
        Assert.Equal(0, existing.Version);
        Assert.Empty(await reopened.GetHistoryAsync(roads));
    }

    [Fact]
    public async Task Assignment_keeps_original_deadline_and_terminal_transitions_clear_overdue()
    {
        var clock = new TestClock(Submitted);
        using var db = await SqliteTestDatabase.CreateAsync(clock);
        var resolvedId = await db.SubmitAsync(SqliteTestDatabase.Requester1);
        var rejectedId = await db.SubmitAsync(SqliteTestDatabase.Requester1);
        clock.Now = Submitted.AddDays(4);

        Assert.True((await db.Transitions.ApplyAsync(new TransitionRequest(resolvedId, RequestStatus.Assigned,
            SqliteTestDatabase.RoadsStaff, AssigneeId: "staff-roads"))).Succeeded);
        var assigned = await db.Repository.GetAsync(resolvedId);
        Assert.Equal(Submitted.AddDays(3), OverduePolicy.DueAt(assigned!));
        Assert.Equal(new ActivityCounts(2, 0, 0, 2), await db.Requests.GetCountsAsync(SqliteTestDatabase.Manager));

        Assert.True((await db.Requests.ReassignAsync(SqliteTestDatabase.RoadsStaff, resolvedId, "staff-roads-2")).Succeeded);
        Assert.True((await db.Requests.AddCommentAsync(SqliteTestDatabase.RoadsStaff2, resolvedId, "Repair in progress")).Succeeded);
        var reassigned = await db.Repository.GetAsync(resolvedId);
        Assert.Equal("staff-roads-2", reassigned!.AssigneeId);
        Assert.Equal(Submitted.AddDays(3), OverduePolicy.DueAt(reassigned));
        Assert.True(OverduePolicy.IsOverdue(reassigned, clock.Now));
        Assert.Single(await db.Requests.GetCommentsAsync(SqliteTestDatabase.Requester1, resolvedId));

        Assert.True((await db.Transitions.ApplyAsync(new TransitionRequest(resolvedId, RequestStatus.Resolved,
            SqliteTestDatabase.RoadsStaff, ResolutionNote: "Fixed"))).Succeeded);
        Assert.True((await db.Transitions.ApplyAsync(new TransitionRequest(rejectedId, RequestStatus.Rejected,
            SqliteTestDatabase.RoadsStaff, Reason: "Duplicate"))).Succeeded);
        Assert.Equal(new ActivityCounts(0, 1, 1, 0), await db.Requests.GetCountsAsync(SqliteTestDatabase.Manager));
        Assert.Equal(2, (await db.Repository.GetHistoryAsync(resolvedId)).Count);
        Assert.Single(await db.Repository.GetHistoryAsync(rejectedId));
    }

    [Fact]
    public void Deadline_uses_elapsed_time_across_offsets_and_future_submissions_are_not_overdue()
    {
        var submitted = Submitted.ToOffset(TimeSpan.FromHours(2));
        var request = new ServiceRequest(Guid.NewGuid(), "req-1", "Roads", "Timezone", "Same instant",
            RequestStatus.Unassigned, submitted, submitted);
        Assert.False(OverduePolicy.IsOverdue(request, Submitted.AddHours(72)));
        Assert.True(OverduePolicy.IsOverdue(request, Submitted.AddHours(72).AddTicks(1)));
        Assert.False(OverduePolicy.IsOverdue(request, Submitted.AddDays(-1)));
    }

    private sealed class TestClock(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;
        public override DateTimeOffset GetUtcNow() => Now;
    }
}
