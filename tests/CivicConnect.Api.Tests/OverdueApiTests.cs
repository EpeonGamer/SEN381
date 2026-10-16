using System.Net;
using CivicConnect.Api.Endpoints;
using CivicConnect.Core.Lifecycle;

namespace CivicConnect.Api.Tests;

/// <summary>FR-013: real HTTP, core and SQLite integration with a controlled clock.</summary>
public class OverdueApiTests
{
    [Fact]
    public async Task Overdue_journey_preserves_privacy_counts_filters_and_lifecycle()
    {
        var submitted = new DateTimeOffset(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);
        var clock = new TestClock(submitted);
        using var api = ApiTarget.InProcess(clock);
        var owner = await api.LoginAsAsync("requester-1");
        var other = await api.LoginAsAsync("requester-2");
        var staff = await api.LoginAsAsync("staff-roads");
        var utilities = await api.LoginAsAsync("staff-utilities");
        var manager = await api.LoginAsAsync("manager-1");
        var createdResponse = await owner.PostJsonAsync("/api/requests",
            new { category = "Roads", title = "Overdue pothole", description = "Test the deadline" });
        Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
        var created = await createdResponse.ReadAsync<RequestSummary>();
        Assert.Equal(submitted.AddHours(72), created.DueAt);
        Assert.False(created.IsOverdue);

        clock.Now = submitted.AddDays(3);
        // The existing authentication cookie expires after eight hours; sign in again after time travel.
        owner = await api.LoginAsAsync("requester-1");
        other = await api.LoginAsAsync("requester-2");
        staff = await api.LoginAsAsync("staff-roads");
        utilities = await api.LoginAsAsync("staff-utilities");
        manager = await api.LoginAsAsync("manager-1");
        var atBoundary = await (await owner.GetAsync($"/api/requests/{created.Id}")).ReadAsync<RequestDetail>();
        Assert.False(atBoundary.Request.IsOverdue);
        Assert.Equal(0, (await (await manager.GetAsync("/api/reports/counts")).ReadAsync<ActivityCountsView>()).Overdue);
        clock.Now = clock.Now.AddTicks(1);
        var detail = await (await owner.GetAsync($"/api/requests/{created.Id}")).ReadAsync<RequestDetail>();
        Assert.True(detail.Request.IsOverdue);
        Assert.Equal(RequestStatus.Unassigned, detail.Request.Status);
        Assert.Empty(detail.History);
        Assert.Equal(created.Id, Assert.Single(await (await staff.GetAsync("/api/requests?overdue=true")).ReadAsync<List<RequestSummary>>()).Id);
        Assert.Empty(await (await other.GetAsync("/api/requests?overdue=true")).ReadAsync<List<RequestSummary>>());
        Assert.Empty(await (await utilities.GetAsync("/api/requests?overdue=true")).ReadAsync<List<RequestSummary>>());
        Assert.Equal(HttpStatusCode.NotFound, (await other.GetAsync($"/api/requests/{created.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await staff.GetAsync("/api/reports/counts?overdue=true")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.Anonymous().GetAsync("/api/requests?overdue=true")).StatusCode);
        Assert.Equal(new ActivityCountsView(1, 0, 0, 1),
            await (await manager.GetAsync("/api/reports/counts?category=Roads&status=Unassigned&overdue=true")).ReadAsync<ActivityCountsView>());
        Assert.Equal(new ActivityCountsView(0, 0, 0, 0),
            await (await manager.GetAsync("/api/reports/counts?category=Water&overdue=true")).ReadAsync<ActivityCountsView>());
        Assert.Empty(await (await owner.GetAsync("/api/requests?overdue=false")).ReadAsync<List<RequestSummary>>());

        Assert.Equal(HttpStatusCode.OK, (await staff.PostJsonAsync($"/api/requests/{created.Id}/transitions",
            new { to = "Assigned", assigneeId = "staff-roads" })).StatusCode);
        var assigned = await (await owner.GetAsync($"/api/requests/{created.Id}")).ReadAsync<RequestDetail>();
        Assert.True(assigned.Request.IsOverdue);
        Assert.Equal(created.DueAt, assigned.Request.DueAt);
        Assert.Equal(HttpStatusCode.OK, (await staff.PostJsonAsync($"/api/requests/{created.Id}/transitions",
            new { to = "Resolved", resolutionNote = "Repaired" })).StatusCode);
        var resolved = await (await owner.GetAsync($"/api/requests/{created.Id}")).ReadAsync<RequestDetail>();
        Assert.False(resolved.Request.IsOverdue);
        Assert.Equal(RequestStatus.Resolved, resolved.Request.Status);
        Assert.Equal(2, resolved.History.Count);
        Assert.Equal("Repaired", resolved.History[^1].ResolutionNote);
        Assert.Equal(new ActivityCountsView(0, 0, 1, 0),
            await (await manager.GetAsync("/api/reports/counts")).ReadAsync<ActivityCountsView>());
    }

    [Fact]
    public async Task Overdue_count_is_not_limited_to_the_first_page_and_invalid_filter_is_rejected()
    {
        var clock = new TestClock(new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero));
        using var api = ApiTarget.InProcess(clock);
        var owner = await api.LoginAsAsync("requester-1");
        var manager = await api.LoginAsAsync("manager-1");
        for (var i = 0; i < 3; i++)
            Assert.Equal(HttpStatusCode.Created, (await owner.PostJsonAsync("/api/requests",
                new { category = "Roads", title = $"Request {i}", description = "Pagination" })).StatusCode);
        clock.Now = clock.Now.AddDays(4);
        owner = await api.LoginAsAsync("requester-1");
        manager = await api.LoginAsAsync("manager-1");
        Assert.Single(await (await owner.GetAsync("/api/requests?overdue=true&limit=1&offset=1")).ReadAsync<List<RequestSummary>>());
        Assert.Equal(new ActivityCountsView(3, 0, 0, 3),
            await (await manager.GetAsync("/api/reports/counts?overdue=true")).ReadAsync<ActivityCountsView>());
        Assert.Equal(HttpStatusCode.BadRequest, (await owner.GetAsync("/api/requests?overdue=invalid")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await manager.GetAsync("/api/reports/counts?overdue=invalid")).StatusCode);
    }

    private sealed class TestClock(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;
        public override DateTimeOffset GetUtcNow() => Now;
    }
}
