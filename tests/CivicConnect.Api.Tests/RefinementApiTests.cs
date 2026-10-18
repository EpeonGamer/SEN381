using System.Net;
using CivicConnect.Api.Endpoints;
using CivicConnect.Core.Lifecycle;
using CivicConnect.Data;

namespace CivicConnect.Api.Tests;

public class RefinementApiTests
{
    [Fact]
    public async Task Utilities_account_reassigns_to_a_colleague_in_its_existing_group()
    {
        using var api = ApiTarget.InProcess();
        var owner = await api.LoginAsAsync("requester-1");
        var utilities = await api.LoginAsAsync("staff-utilities");
        // Test-only reference data; production/demo group permissions are unchanged.
        await SqliteSetup.AddUserAsync(api.ConnectionString, "utility-colleague", "Staff");
        await SqliteSetup.AddUserToGroupAsync(api.ConnectionString, "utility-colleague", "Utilities-Team");
        var request = await Submit(owner, "Water");
        var url = $"/api/requests/{request.Id}";
        Assert.Equal(HttpStatusCode.OK, (await utilities.PostJsonAsync(url + "/transitions", new { to = "Assigned", assigneeId = "staff-utilities" })).StatusCode);
        Assert.Equal("utility-colleague", Assert.Single(await (await utilities.GetAsync(url + "/eligible-assignees")).ReadAsync<List<string>>()));
        Assert.Equal(HttpStatusCode.NoContent, (await utilities.PostJsonAsync(url + "/reassign", new { assigneeId = "utility-colleague", expectedVersion = 1 })).StatusCode);
        Assert.Equal("utility-colleague", (await (await owner.GetAsync(url)).ReadAsync<RequestDetail>()).Request.AssigneeId);
    }

    [Theory]
    [InlineData("Assigned")]
    [InlineData("Rejected")]
    [InlineData("Resolved")]
    public async Task Managers_cannot_call_any_operational_transition(string to)
    {
        using var api = ApiTarget.InProcess();
        var owner = await api.LoginAsAsync("requester-1");
        var manager = await api.LoginAsAsync("manager-1");
        var request = await Submit(owner);
        var result = await manager.PostJsonAsync($"/api/requests/{request.Id}/transitions",
            new { to, assigneeId = "staff-roads", reason = "Duplicate", resolutionNote = "Done" });
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        Assert.Equal(RequestStatus.Unassigned, (await (await owner.GetAsync($"/api/requests/{request.Id}")).ReadAsync<RequestDetail>()).Request.Status);
    }

    private static async Task<RequestSummary> Submit(HttpClient client, string category = "Roads", string title = "Repair marker") =>
        await (await client.PostJsonAsync("/api/requests", new { category, title, description = "Searchable pipe marker" })).ReadAsync<RequestSummary>();

    [Fact]
    public async Task Assignment_directory_reassignment_comments_and_stale_version_work_together()
    {
        using var api = ApiTarget.InProcess();
        var owner = await api.LoginAsAsync("requester-1");
        var staff = await api.LoginAsAsync("staff-roads");
        var request = await Submit(owner);
        var url = $"/api/requests/{request.Id}";
        var targets = await (await staff.GetAsync(url + "/eligible-assignees")).ReadAsync<List<string>>();
        Assert.Equal(2, targets.Count);
        Assert.Contains("staff-roads", targets);
        Assert.Contains("staff-roads-2", targets);
        Assert.Equal(HttpStatusCode.Conflict, (await staff.PostJsonAsync(url + "/reassign", new { assigneeId = "staff-roads-2" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await staff.PostJsonAsync(url + "/transitions", new { to = "Assigned", assigneeId = "staff-roads" })).StatusCode);
        var before = await (await staff.GetAsync(url)).ReadAsync<RequestDetail>();
        Assert.Equal("staff-roads-2", Assert.Single(await (await staff.GetAsync(url + "/eligible-assignees")).ReadAsync<List<string>>()));
        Assert.Equal(HttpStatusCode.NoContent, (await staff.PostJsonAsync(url + "/reassign", new { assigneeId = "staff-roads-2", expectedVersion = before.Request.Version })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await staff.PostJsonAsync(url + "/reassign", new { assigneeId = "staff-roads", expectedVersion = before.Request.Version })).StatusCode);
        const string text = "<script>alert('comment')</script> & update";
        Assert.Equal(HttpStatusCode.NoContent, (await staff.PostJsonAsync(url + "/comments", new { content = text })).StatusCode);
        var after = await (await owner.GetAsync(url)).ReadAsync<RequestDetail>();
        Assert.Equal("staff-roads-2", after.Request.AssigneeId);
        Assert.Equal(before.Request.Version + 1, after.Request.Version);
        Assert.Equal(text, Assert.Single(after.Comments).Content);
        Assert.Equal("staff-roads", after.Comments[0].AuthorId);
        Assert.Single(after.History);
        Assert.Single(await (await staff.GetAsync("/api/requests?search=staff-roads-2")).ReadAsync<List<RequestSummary>>());
        Assert.Single(await (await owner.GetAsync($"/api/requests?search={request.Id}")).ReadAsync<List<RequestSummary>>());
        Assert.Equal(HttpStatusCode.BadRequest, (await staff.PostJsonAsync(url + "/comments", new { content = " " })).StatusCode);
    }

    [Theory]
    [InlineData("manager-1")]
    [InlineData("requester-1")]
    [InlineData("staff-utilities")]
    public async Task Unauthorised_callers_cannot_reassign_or_browse_staff(string userId)
    {
        using var api = ApiTarget.InProcess();
        var owner = await api.LoginAsAsync("requester-1");
        var request = await Submit(owner);
        var caller = await api.LoginAsAsync(userId);
        var url = $"/api/requests/{request.Id}";
        Assert.Equal(userId == "staff-utilities" ? HttpStatusCode.NotFound : HttpStatusCode.Forbidden,
            (await caller.GetAsync(url + "/eligible-assignees")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await caller.PostJsonAsync(url + "/reassign", new { assigneeId = "staff-roads" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await caller.PostJsonAsync(url + "/comments", new { content = "Refused" })).StatusCode);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("manager-1")]
    [InlineData("requester-1")]
    [InlineData("staff-utilities")]
    public async Task Ineligible_assignee_is_rejected_without_changing_responsibility(string target)
    {
        using var api = ApiTarget.InProcess();
        var owner = await api.LoginAsAsync("requester-1");
        var staff = await api.LoginAsAsync("staff-roads");
        var request = await Submit(owner);
        var url = $"/api/requests/{request.Id}";
        await staff.PostJsonAsync(url + "/transitions", new { to = "Assigned", assigneeId = "staff-roads" });
        var response = await staff.PostJsonAsync(url + "/reassign", new { assigneeId = target });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("InvalidAssignee", (await response.ReadAsync<ApiError>()).Error);
        Assert.Equal("staff-roads", (await (await owner.GetAsync(url)).ReadAsync<RequestDetail>()).Request.AssigneeId);
    }

    [Fact]
    public async Task Combined_filters_scope_pagination_and_counts_match()
    {
        using var api = ApiTarget.InProcess();
        var owner = await api.LoginAsAsync("requester-1");
        var other = await api.LoginAsAsync("requester-2");
        var manager = await api.LoginAsAsync("manager-1");
        for (var i = 0; i < 7; i++) await Submit(owner);
        await Submit(owner, "Water");
        await Submit(owner, "Roads", "Other title");
        const string query = "category=Roads&status=Unassigned&search=REPAIR";
        var page1 = await (await manager.GetAsync("/api/requests?" + query + "&limit=5")).ReadAsync<List<RequestSummary>>();
        var page2 = await (await manager.GetAsync("/api/requests?" + query + "&limit=5&offset=5")).ReadAsync<List<RequestSummary>>();
        Assert.Equal(5, page1.Count);
        Assert.Equal(2, page2.Count);
        Assert.Equal(7, page1.Concat(page2).Select(r => r.Id).Distinct().Count());
        Assert.Equal(7, (await (await manager.GetAsync("/api/reports/counts?" + query)).ReadAsync<ActivityCountsView>()).Open);
        Assert.Equal(9, (await (await manager.GetAsync("/api/requests?search=pipe")).ReadAsync<List<RequestSummary>>()).Count);
        var sorted = await (await manager.GetAsync("/api/requests?sortByCategory=true")).ReadAsync<List<RequestSummary>>();
        Assert.Equal(sorted.Select(r => r.Category).OrderBy(c => c, StringComparer.OrdinalIgnoreCase), sorted.Select(r => r.Category));
        Assert.Empty(await (await manager.GetAsync("/api/requests?search=%27%20OR%201%3D1--")).ReadAsync<List<RequestSummary>>());
        Assert.Empty(await (await other.GetAsync("/api/requests?" + query)).ReadAsync<List<RequestSummary>>());
        Assert.Equal(HttpStatusCode.BadRequest, (await manager.GetAsync("/api/requests?status=999999999999999999999999")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await manager.GetAsync("/api/requests?status=Assigned%2CRejected")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await manager.GetAsync("/api/requests?search=" + new string('x', 121))).StatusCode);
    }
}
