using CivicConnect.Core.Authorization;
using CivicConnect.Core.Lifecycle;
using CivicConnect.Core.Requests;

namespace CivicConnect.Core.Tests;

/// <summary>
/// Black-box functional tests: inputs and expected results come from the requirements and acceptance
/// criteria (PED 1.5), not from the code. All run against a real SQLite database (SqliteTestDatabase).
/// Techniques: D = derived from an acceptance criterion, EP = equivalence partitioning,
/// DT = decision table, ST = state transition.
/// </summary>
public class BlackBoxTests
{
    // D1 — FR-004/FR-005: the requester can see their own request and its current status.
    [Fact]
    public async Task D1_Requester_can_review_own_request()
    {
        using var db = await SqliteTestDatabase.CreateAsync();
        var id = await db.SubmitAsync(SqliteTestDatabase.Requester1);

        var detail = await db.Requests.GetAsync(SqliteTestDatabase.Requester1, id);
        var list = await db.Requests.ListAsync(SqliteTestDatabase.Requester1);

        Assert.NotNull(detail);
        Assert.Equal(RequestStatus.Unassigned, detail!.Status);
        Assert.Equal(id, Assert.Single(list).Id);
    }

    // D2 — FR-005 AC: "When no requests exist, an empty history list is returned" (not an error).
    [Fact]
    public async Task D2_Requester_with_no_requests_gets_an_empty_list()
    {
        using var db = await SqliteTestDatabase.CreateAsync();
        await db.SubmitAsync(SqliteTestDatabase.Requester1); // someone else's data exists

        Assert.Empty(await db.Requests.ListAsync(SqliteTestDatabase.Requester2));
    }

    // EP1 — FR-002/FR-003 submission input partitions. One representative value per partition.
    //   Valid:   all fields present + known category; known category in different letter case.
    //   Invalid: blank title, blank description, blank category, unknown category.
    [Theory]
    [InlineData("Roads", "Pothole", "Deep hole", RequestFailure.None)]             // valid
    [InlineData("rOADS", "Pothole", "Deep hole", RequestFailure.None)]             // valid: categories are case-insensitive
    [InlineData("Roads", "   ", "Deep hole", RequestFailure.InvalidInput)]         // invalid: whitespace-only title
    [InlineData("Roads", "Pothole", "", RequestFailure.InvalidInput)]              // invalid: empty description
    [InlineData("", "Pothole", "Deep hole", RequestFailure.InvalidInput)]          // invalid: no category
    [InlineData("Spaceships", "Pothole", "Deep hole", RequestFailure.InvalidCategory)] // invalid: not in catalogue
    public async Task EP1_Submission_input_partitions(string category, string title, string description, RequestFailure expected)
    {
        using var db = await SqliteTestDatabase.CreateAsync();

        var result = await db.Requests.SubmitAsync(SqliteTestDatabase.Requester1, category, title, description);

        Assert.Equal(expected, result.Failure);
        var stored = await db.Requests.ListAsync(SqliteTestDatabase.Requester1);
        Assert.Equal(expected == RequestFailure.None ? 1 : 0, stored.Count); // invalid input stores nothing
    }

    // EP2 — FR-001: only the Requester role may submit. Staff and Manager are the invalid partition.
    [Theory]
    [InlineData("staff-roads")]
    [InlineData("mgr-1")]
    public async Task EP2_Non_requesters_cannot_submit(string actorId)
    {
        using var db = await SqliteTestDatabase.CreateAsync();
        var actor = new ActorContext(actorId, ActorRole.Requester); // claims Requester; the stored role wins

        var result = await db.Requests.SubmitAsync(actor, "Roads", "Pothole", "Deep hole");

        Assert.Equal(RequestFailure.NotAuthorised, result.Failure);
    }

    // DT1 — FR-001, FR-008, FR-009, DEF-01: who may assign a Roads request?
    //
    //   Rule | Role      | Staff group covers "Roads" | Expected
    //   R1   | Requester | n/a                        | NotAuthorised
    //   R2   | Manager   | n/a                        | NotAuthorised
    //   R3   | Staff     | No  (General-Team)         | NotAuthorised  (DEF-01 regression)
    //   R4   | Staff     | Yes (Roads-Team)           | Succeeds
    [Theory]
    [InlineData("req-1", false)]          // R1
    [InlineData("mgr-1", false)]          // R2
    [InlineData("staff-general", false)]  // R3
    [InlineData("staff-roads", true)]     // R4
    public async Task DT1_Assignment_decision_table(string actorId, bool expectedToSucceed)
    {
        using var db = await SqliteTestDatabase.CreateAsync();
        var id = await db.SubmitAsync(SqliteTestDatabase.Requester1, "Roads");
        var actor = new ActorContext(actorId, ActorRole.Staff); // claims Staff; the stored role wins

        var result = await db.Transitions.ApplyAsync(
            new TransitionRequest(id, RequestStatus.Assigned, actor, AssigneeId: "staff-roads"));

        Assert.Equal(expectedToSucceed, result.Succeeded);
        if (!expectedToSucceed) Assert.Equal(TransitionFailure.NotAuthorised, result.Failure);
        var stored = await db.Repository.GetAsync(id);
        Assert.Equal(expectedToSucceed ? RequestStatus.Assigned : RequestStatus.Unassigned, stored!.Status);
    }

    // ST1 — FR-011, FR-015, FR-004: the full approved path Unassigned -> Assigned -> Resolved, persisted,
    // with the requester seeing the final status, and Resolved behaving as a terminal state.
    [Fact]
    public async Task ST1_Request_follows_approved_path_and_resolved_is_terminal()
    {
        using var db = await SqliteTestDatabase.CreateAsync();
        var id = await db.SubmitAsync(SqliteTestDatabase.Requester1, "Roads");
        var staff = SqliteTestDatabase.RoadsStaff;

        var assign = await db.Transitions.ApplyAsync(new TransitionRequest(id, RequestStatus.Assigned, staff, AssigneeId: "staff-roads"));
        var resolve = await db.Transitions.ApplyAsync(new TransitionRequest(id, RequestStatus.Resolved, staff, ResolutionNote: "Patched"));
        var reopen = await db.Transitions.ApplyAsync(new TransitionRequest(id, RequestStatus.Assigned, staff, AssigneeId: "staff-roads"));

        Assert.True(assign.Succeeded);
        Assert.True(resolve.Succeeded);
        Assert.Equal(TransitionFailure.InvalidTransition, reopen.Failure);

        var seenByRequester = await db.Requests.GetAsync(SqliteTestDatabase.Requester1, id);
        Assert.Equal(RequestStatus.Resolved, seenByRequester!.Status);
        var history = await db.Requests.GetHistoryAsync(SqliteTestDatabase.Requester1, id);
        Assert.Equal(new[] { RequestStatus.Assigned, RequestStatus.Resolved }, history.Select(h => h.To));
        Assert.Equal("Patched", history[^1].ResolutionNote);
    }
}
