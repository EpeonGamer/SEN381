using System.Net;
using CivicConnect.Api.Endpoints;
using CivicConnect.Core.Lifecycle;

namespace CivicConnect.Api.Tests;

/// <summary>
/// E2E/system tests: complete user journeys across login, API, Core rules and the database, using only
/// the public HTTP interface (the same calls the frontend makes). They run in-process in CI and against
/// staging when CIVICCONNECT_E2E_BASEURL / CIVICCONNECT_E2E_PASSWORD are set (see docs/deployment).
/// Limitation: the browser UI itself is not automated; it is demonstrated manually.
/// </summary>
public class EndToEndTests
{
    // E2E1 — critical journey (FR-002, FR-004, FR-009, FR-011, NFR-004):
    // requester submits -> staff assigns -> staff resolves -> requester sees Resolved and the note.
    [Fact]
    public async Task E2E1_Request_is_submitted_assigned_resolved_and_the_requester_sees_the_outcome()
    {
        using var system = ApiTarget.FromEnvironment();
        var requester = await system.LoginAsAsync("requester-1");
        var staff = await system.LoginAsAsync("staff-roads");
        var title = $"E2E pothole {Guid.NewGuid():N}"[..20];

        var created = await (await requester.PostJsonAsync("/api/requests",
            new { category = "Roads", title, description = "Deep pothole outside number 12" })).ReadAsync<RequestSummary>();
        var assigned = await staff.PostJsonAsync($"/api/requests/{created.Id}/transitions",
            new { to = "Assigned", assigneeId = "staff-roads" });
        var resolved = await staff.PostJsonAsync($"/api/requests/{created.Id}/transitions",
            new { to = "Resolved", resolutionNote = "Pothole filled" });

        Assert.Equal(HttpStatusCode.OK, assigned.StatusCode);
        Assert.Equal(HttpStatusCode.OK, resolved.StatusCode);

        var myRequests = await (await requester.GetAsync("/api/requests?status=Resolved")).ReadAsync<List<RequestSummary>>();
        Assert.Contains(myRequests, r => r.Id == created.Id && r.Title == title);

        var detail = await (await requester.GetAsync($"/api/requests/{created.Id}")).ReadAsync<RequestDetail>();
        Assert.Equal(RequestStatus.Resolved, detail.Request.Status);
        Assert.Equal("staff-roads", detail.Request.AssigneeId);
        Assert.Equal(new[] { RequestStatus.Assigned, RequestStatus.Resolved }, detail.History.Select(h => h.To));
        Assert.Equal("Pothole filled", detail.History[^1].ResolutionNote);
    }

    // E2E2 — feedback journey (FR-007): a rejected request shows the requester the status and the reason.
    [Fact]
    public async Task E2E2_Rejected_request_shows_the_requester_the_reason()
    {
        using var system = ApiTarget.FromEnvironment();
        var requester = await system.LoginAsAsync("requester-1");
        var staff = await system.LoginAsAsync("staff-utilities");

        var created = await (await requester.PostJsonAsync("/api/requests",
            new { category = "Water", title = "Low pressure", description = "Low water pressure since Monday" })).ReadAsync<RequestSummary>();
        var rejected = await staff.PostJsonAsync($"/api/requests/{created.Id}/transitions",
            new { to = "Rejected", reason = "Municipal maintenance scheduled; not a fault" });

        Assert.Equal(HttpStatusCode.OK, rejected.StatusCode);
        var detail = await (await requester.GetAsync($"/api/requests/{created.Id}")).ReadAsync<RequestDetail>();
        Assert.Equal(RequestStatus.Rejected, detail.Request.Status);
        Assert.Equal("Municipal maintenance scheduled; not a fault", Assert.Single(detail.History).Reason);
    }
}
