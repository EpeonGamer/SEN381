using System.Net;
using CivicConnect.Api.Endpoints;
using CivicConnect.Core.Lifecycle;

namespace CivicConnect.Api.Tests;

/// <summary>
/// API/integration tests: the real HTTP pipeline (authentication, authorization, JSON, endpoints) talking to
/// the real Core services and a real SQLite file. Each test owns a fresh database.
/// </summary>
public class ApiIntegrationTests
{
    // API1 — NFR-005: protected endpoints reject callers who are not signed in.
    [Theory]
    [InlineData("GET", "/api/requests")]
    [InlineData("POST", "/api/requests")]
    [InlineData("GET", "/api/categories")]
    [InlineData("GET", "/api/reports/counts")]
    public async Task API1_Unauthenticated_calls_get_401(string method, string url)
    {
        using var api = ApiTarget.InProcess();

        var response = await api.Anonymous().SendAsync(new HttpRequestMessage(new HttpMethod(method), url));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // API2 — NFR-002/RISK-001: wrong password and unknown user get the same answer (no user enumeration).
    [Fact]
    public async Task API2_Wrong_password_and_unknown_user_are_indistinguishable()
    {
        using var api = ApiTarget.InProcess();
        var client = api.Anonymous();

        var wrongPassword = await client.PostJsonAsync("/api/auth/login", new { userId = "requester-1", password = "wrong-password" });
        var unknownUser = await client.PostJsonAsync("/api/auth/login", new { userId = "nobody", password = "wrong-password" });

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownUser.StatusCode);
        Assert.Equal((await wrongPassword.ReadAsync<ApiError>()).Error, (await unknownUser.ReadAsync<ApiError>()).Error);
    }

    // API3 — FR-002, FR-003, FR-004 and DEF-08 regression: a submission is stored, starts Unassigned,
    // is readable back, and uses the catalogue spelling of the category.
    [Fact]
    public async Task API3_Submitted_request_is_created_and_readable_back()
    {
        using var api = ApiTarget.InProcess();
        var requester = await api.LoginAsAsync("requester-1");

        var response = await requester.PostJsonAsync("/api/requests",
            new { category = "roads", title = "Pothole", description = "Deep pothole on Main Rd" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.ReadAsync<RequestSummary>();
        Assert.Equal(RequestStatus.Unassigned, created.Status);
        Assert.Equal("Roads", created.Category); // DEF-08: not "roads"
        Assert.Equal($"/api/requests/{created.Id}", response.Headers.Location!.OriginalString);

        var detail = await (await requester.GetAsync($"/api/requests/{created.Id}")).ReadAsync<RequestDetail>();
        Assert.Equal("requester-1", detail.Request.RequesterId);
        Assert.Equal("Deep pothole on Main Rd", detail.Description);
    }

    // API4 — NFR-007, FR-005: another requester cannot read the request (404, not 403, so its existence
    // is not confirmed) and does not see it in their list.
    [Fact]
    public async Task API4_Requester_cannot_see_another_requesters_request()
    {
        using var api = ApiTarget.InProcess();
        var owner = await api.LoginAsAsync("requester-1");
        var other = await api.LoginAsAsync("requester-2");
        var created = await (await owner.PostJsonAsync("/api/requests",
            new { category = "Water", title = "Leak", description = "Pipe leaking" })).ReadAsync<RequestSummary>();

        var detail = await other.GetAsync($"/api/requests/{created.Id}");
        var list = await (await other.GetAsync("/api/requests")).ReadAsync<List<RequestSummary>>();

        Assert.Equal(HttpStatusCode.NotFound, detail.StatusCode);
        Assert.Empty(list);
    }

    // API5 — FR-001, FR-015: only Staff may change status; Requester and Manager get 403 and nothing changes.
    [Theory]
    [InlineData("requester-1")]
    [InlineData("manager-1")]
    public async Task API5_Non_staff_cannot_change_status(string userId)
    {
        using var api = ApiTarget.InProcess();
        var owner = await api.LoginAsAsync("requester-1");
        var created = await (await owner.PostJsonAsync("/api/requests",
            new { category = "Roads", title = "Pothole", description = "Deep hole" })).ReadAsync<RequestSummary>();
        var caller = await api.LoginAsAsync(userId);

        var response = await caller.PostJsonAsync($"/api/requests/{created.Id}/transitions",
            new { to = "Assigned", assigneeId = "staff-roads" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var after = await (await owner.GetAsync($"/api/requests/{created.Id}")).ReadAsync<RequestDetail>();
        Assert.Equal(RequestStatus.Unassigned, after.Request.Status);
    }

    // API6 — FR-002 API contract, boundary value analysis on title length (limit 120 characters).
    //   0 (empty) -> 400 · 1 -> 201 · 120 (on the limit) -> 201 · 121 (just over) -> 400
    [Theory]
    [InlineData(0, HttpStatusCode.BadRequest)]
    [InlineData(1, HttpStatusCode.Created)]
    [InlineData(RequestEndpoints.MaxTitleLength, HttpStatusCode.Created)]
    [InlineData(RequestEndpoints.MaxTitleLength + 1, HttpStatusCode.BadRequest)]
    public async Task API6_Title_length_boundaries(int length, HttpStatusCode expected)
    {
        using var api = ApiTarget.InProcess();
        var requester = await api.LoginAsAsync("requester-1");

        var response = await requester.PostJsonAsync("/api/requests",
            new { category = "Roads", title = new string('T', length), description = "Boundary test" });

        Assert.Equal(expected, response.StatusCode);
    }

    // API7 — FR-007, FR-011, FR-015: invalid lifecycle requests are refused with a clear reason.
    [Fact]
    public async Task API7_Invalid_status_changes_are_refused_with_a_reason()
    {
        using var api = ApiTarget.InProcess();
        var owner = await api.LoginAsAsync("requester-1");
        var staff = await api.LoginAsAsync("staff-roads");
        var created = await (await owner.PostJsonAsync("/api/requests",
            new { category = "Roads", title = "Pothole", description = "Deep hole" })).ReadAsync<RequestSummary>();
        var url = $"/api/requests/{created.Id}/transitions";

        var rejectWithoutReason = await staff.PostJsonAsync(url, new { to = "Rejected", reason = "  " });
        var resolveUnassigned = await staff.PostJsonAsync(url, new { to = "Resolved", resolutionNote = "done" });

        Assert.Equal(HttpStatusCode.BadRequest, rejectWithoutReason.StatusCode);
        Assert.Equal("MissingReason", (await rejectWithoutReason.ReadAsync<ApiError>()).Error);
        Assert.Equal(HttpStatusCode.Conflict, resolveUnassigned.StatusCode);
        Assert.Equal("InvalidTransition", (await resolveUnassigned.ReadAsync<ApiError>()).Error);
    }

    // API8 — FR-012, FR-013: managers get counts that match the stored data; other roles get 403.
    [Fact]
    public async Task API8_Manager_counts_match_stored_requests_and_are_manager_only()
    {
        using var api = ApiTarget.InProcess();
        var requester = await api.LoginAsAsync("requester-1");
        var staff = await api.LoginAsAsync("staff-roads");
        var manager = await api.LoginAsAsync("manager-1");
        var ids = new List<Guid>();
        for (var i = 0; i < 3; i++)
            ids.Add((await (await requester.PostJsonAsync("/api/requests",
                new { category = "Roads", title = $"Pothole {i}", description = "Deep hole" })).ReadAsync<RequestSummary>()).Id);
        await staff.PostJsonAsync($"/api/requests/{ids[0]}/transitions", new { to = "Rejected", reason = "Duplicate" });

        var counts = await (await manager.GetAsync("/api/reports/counts")).ReadAsync<ActivityCountsView>();
        var staffAttempt = await staff.GetAsync("/api/reports/counts");

        Assert.Equal(new ActivityCountsView(Open: 2, Rejected: 1, Resolved: 0), counts);
        Assert.Equal(HttpStatusCode.Forbidden, staffAttempt.StatusCode);
    }

    // API10 — DEF-10 regression: a malformed query value is the caller's mistake, so it must be a 400 in every
    // environment. Development makes ASP.NET Core throw on bad input, which the error handler used to turn into a 500.
    [Theory]
    [InlineData("Testing", "/api/requests?limit=abc")]
    [InlineData("Development", "/api/requests?limit=abc")]
    [InlineData("Development", "/api/requests?overdue=maybe")]
    public async Task API10_Malformed_query_values_get_400_not_500(string environment, string url)
    {
        using var api = ApiTarget.InProcess(environment: environment);
        var requester = await api.LoginAsAsync("requester-1");

        var response = await requester.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // API9 — release identification (M3 staging): /health is public and reports the build version.
    [Fact]
    public async Task API9_Health_reports_status_and_release_version()
    {
        using var api = ApiTarget.InProcess();

        var response = await api.Anonymous().GetAsync("/health");
        var body = await response.ReadAsync<Dictionary<string, string>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", body["status"]);
        Assert.StartsWith("0.3.0", body["version"]);
    }
}
