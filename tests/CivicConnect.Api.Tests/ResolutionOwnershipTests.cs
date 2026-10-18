using System.Net;
using CivicConnect.Api.Endpoints;
using CivicConnect.Core.Lifecycle;
using CivicConnect.Data;

namespace CivicConnect.Api.Tests;

public class ResolutionOwnershipTests
{
    [Theory]
    [InlineData("staff-roads")]
    [InlineData("staff-utilities")]
    public async Task Non_assignee_must_reassign_to_self_before_resolving(string userId)
    {
        using var api = ApiTarget.InProcess();
        var owner = await api.LoginAsAsync("requester-1");
        var assigner = await api.LoginAsAsync("staff-roads");
        // Give the third staff member the same category scope only in this disposable test database.
        await SqliteSetup.AddUserToGroupAsync(api.ConnectionString, "staff-utilities", "Roads-Team");
        var caller = await api.LoginAsAsync(userId);
        var request = await (await owner.PostJsonAsync("/api/requests", new
        {
            category = "Roads", title = "Assignee-only resolution", description = "Ownership regression"
        })).ReadAsync<RequestSummary>();
        var url = $"/api/requests/{request.Id}";
        Assert.Equal(HttpStatusCode.OK, (await assigner.PostJsonAsync(url + "/transitions",
            new { to = "Assigned", assigneeId = "staff-roads-2" })).StatusCode);
        var before = await (await owner.GetAsync(url)).ReadAsync<RequestDetail>();

        // Supplying an assignee in the resolution body cannot bypass stored ownership.
        var refused = await caller.PostJsonAsync(url + "/transitions",
            new { to = "Resolved", assigneeId = userId, resolutionNote = "Attempted closure" });
        Assert.Equal(HttpStatusCode.Forbidden, refused.StatusCode);
        var error = await refused.ReadAsync<ApiError>();
        Assert.Equal("NotAssigned", error.Error);
        Assert.Contains("when you are assigned", error.Detail!);
        var unchanged = await (await owner.GetAsync(url)).ReadAsync<RequestDetail>();
        Assert.Equal(before.Request, unchanged.Request);
        Assert.Single(unchanged.History);

        Assert.Equal(HttpStatusCode.NoContent, (await caller.PostJsonAsync(url + "/reassign",
            new { assigneeId = userId, expectedVersion = before.Request.Version })).StatusCode);
        var formerAssignee = await api.LoginAsAsync("staff-roads-2");
        var staleAttempt = await formerAssignee.PostJsonAsync(url + "/transitions",
            new { to = "Resolved", resolutionNote = "Old detail still open" });
        Assert.Equal(HttpStatusCode.Forbidden, staleAttempt.StatusCode);
        Assert.Equal("NotAssigned", (await staleAttempt.ReadAsync<ApiError>()).Error);
        Assert.Equal(HttpStatusCode.OK, (await caller.PostJsonAsync(url + "/transitions",
            new { to = "Resolved", resolutionNote = "Resolved by current assignee" })).StatusCode);
        var after = await (await owner.GetAsync(url)).ReadAsync<RequestDetail>();
        Assert.Equal(RequestStatus.Resolved, after.Request.Status);
        Assert.Equal(userId, after.Request.AssigneeId);
        Assert.Equal(userId, after.History[^1].ActorId);
        Assert.Equal(2, after.History.Count);
    }

    [Fact]
    public async Task Assigned_staff_can_resolve_without_reassignment()
    {
        using var api = ApiTarget.InProcess();
        var owner = await api.LoginAsAsync("requester-1");
        var assigner = await api.LoginAsAsync("staff-roads");
        var assignee = await api.LoginAsAsync("staff-roads-2");
        var request = await (await owner.PostJsonAsync("/api/requests", new
        {
            category = "Roads", title = "Assigned colleague", description = "Staff 1 assigns Staff 2"
        })).ReadAsync<RequestSummary>();
        var url = $"/api/requests/{request.Id}/transitions";
        Assert.Equal(HttpStatusCode.OK, (await assigner.PostJsonAsync(url,
            new { to = "Assigned", assigneeId = "staff-roads-2" })).StatusCode);
        var missingNote = await assignee.PostJsonAsync(url, new { to = "Resolved", resolutionNote = " " });
        Assert.Equal(HttpStatusCode.BadRequest, missingNote.StatusCode);
        Assert.Equal("MissingResolutionNote", (await missingNote.ReadAsync<ApiError>()).Error);
        Assert.Equal(HttpStatusCode.OK, (await assignee.PostJsonAsync(url,
            new { to = "Resolved", resolutionNote = "Repaired" })).StatusCode);
    }
}
