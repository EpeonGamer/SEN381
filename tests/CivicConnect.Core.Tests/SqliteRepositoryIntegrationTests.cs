using CivicConnect.Core.Authorization;
using CivicConnect.Core.Lifecycle;
using CivicConnect.Core.Requests;
using CivicConnect.Data;
using Microsoft.Data.Sqlite;
using Xunit;

namespace CivicConnect.Core.Tests;

/// <summary>
/// A real SQLite file database per test (ADR-07), with seeded reference data.
/// Shared by the integration tests and by the black-box tests that need a real repository.
/// </summary>
public sealed class SqliteTestDatabase : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"civicconnect-test-{Guid.NewGuid():N}.db");

    public static readonly ActorContext Requester1 = new("req-1", ActorRole.Requester);
    public static readonly ActorContext Requester2 = new("req-2", ActorRole.Requester);
    public static readonly ActorContext RoadsStaff = new("staff-roads", ActorRole.Staff);
    public static readonly ActorContext RoadsStaff2 = new("staff-roads-2", ActorRole.Staff);
    public static readonly ActorContext GeneralStaff = new("staff-general", ActorRole.Staff);
    public static readonly ActorContext Manager = new("mgr-1", ActorRole.Manager);

    public string ConnectionString => $"Data Source={_path}";
    public SqliteRequestRepository Repository { get; }
    public RequestService Requests { get; }
    public TransitionService Transitions { get; }

    private SqliteTestDatabase(TimeProvider? clock = null)
    {
        Repository = new SqliteRequestRepository(ConnectionString, clock);
        Requests = new RequestService(Repository, clock: clock);
        Transitions = new TransitionService(Repository, clock: clock);
    }

    public static async Task<SqliteTestDatabase> CreateAsync(TimeProvider? clock = null)
    {
        var db = new SqliteTestDatabase(clock);
        var cs = db.ConnectionString;
        await SqliteSchema.EnsureCreatedAsync(cs);
        await SqliteSetup.AddCategoryAsync(cs, "Roads");
        await SqliteSetup.AddCategoryAsync(cs, "General");
        await SqliteSetup.AddStaffGroupAsync(cs, "Roads-Team", "Roads");
        await SqliteSetup.AddStaffGroupAsync(cs, "General-Team", "General");
        foreach (var requester in new[] { "req-1", "req-2" }) await SqliteSetup.AddUserAsync(cs, requester, "Requester");
        foreach (var staff in new[] { "staff-roads", "staff-roads-2", "staff-general" }) await SqliteSetup.AddUserAsync(cs, staff, "Staff");
        await SqliteSetup.AddUserAsync(cs, "mgr-1", "Manager");
        await SqliteSetup.AddUserToGroupAsync(cs, "staff-roads", "Roads-Team");
        await SqliteSetup.AddUserToGroupAsync(cs, "staff-roads-2", "Roads-Team");
        await SqliteSetup.AddUserToGroupAsync(cs, "staff-general", "General-Team");
        return db;
    }

    public async Task<Guid> SubmitAsync(ActorContext requester, string category = "Roads")
    {
        var result = await Requests.SubmitAsync(requester, category, "Pothole", "Deep pothole outside number 12");
        Assert.True(result.Succeeded);
        return result.RequestId!.Value;
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { File.Delete(_path); } catch (IOException) { /* temp file; ignore */ }
    }
}

/// <summary>
/// Integration tests at the Core ↔ persistence boundary (real SQLite).
/// Basis: FR-002, FR-008, NFR-004, NFR-007; ADR-05, ADR-07; RISK-M2-03, RISK-M2-04.
/// </summary>
public class SqliteRepositoryIntegrationTests
{
    [Fact]
    public async Task Submitted_request_is_stored_with_owner_category_and_initial_status()
    {
        using var db = await SqliteTestDatabase.CreateAsync();

        var id = await db.SubmitAsync(SqliteTestDatabase.Requester1);

        var stored = await db.Repository.GetAsync(id);
        Assert.NotNull(stored);
        Assert.Equal("req-1", stored!.RequesterId);
        Assert.Equal("Roads", stored.Category);
        Assert.Equal(RequestStatus.Unassigned, stored.Status);
    }

    [Fact]
    public async Task Requester_cannot_read_another_requesters_request_from_the_database()
    {
        using var db = await SqliteTestDatabase.CreateAsync();
        var id = await db.SubmitAsync(SqliteTestDatabase.Requester1);

        Assert.Null(await db.Requests.GetAsync(SqliteTestDatabase.Requester2, id));
        Assert.Empty(await db.Requests.ListAsync(SqliteTestDatabase.Requester2));
        Assert.Single(await db.Requests.ListAsync(SqliteTestDatabase.Requester1));
    }

    [Fact]
    public async Task Staff_only_see_requests_in_their_groups_categories()
    {
        using var db = await SqliteTestDatabase.CreateAsync();
        var id = await db.SubmitAsync(SqliteTestDatabase.Requester1, "Roads");

        Assert.NotNull(await db.Requests.GetAsync(SqliteTestDatabase.RoadsStaff, id));
        Assert.Null(await db.Requests.GetAsync(SqliteTestDatabase.GeneralStaff, id));
        Assert.Empty(await db.Requests.ListAsync(SqliteTestDatabase.GeneralStaff));
    }

    [Fact]
    public async Task Failed_audit_write_rolls_back_the_status_change()
    {
        using var db = await SqliteTestDatabase.CreateAsync();
        var id = await db.SubmitAsync(SqliteTestDatabase.Requester1);
        await using (var connection = new SqliteConnection(db.ConnectionString))
        {
            await connection.OpenAsync();
            await using var trigger = connection.CreateCommand();
            trigger.CommandText = "CREATE TRIGGER fail_audit BEFORE INSERT ON transition_history " +
                                  "BEGIN SELECT RAISE(ABORT, 'simulated audit failure'); END;";
            await trigger.ExecuteNonQueryAsync();
        }

        await Assert.ThrowsAsync<SqliteException>(() => db.Transitions.ApplyAsync(
            new TransitionRequest(id, RequestStatus.Assigned, SqliteTestDatabase.RoadsStaff, AssigneeId: "staff-roads-2")));

        var stored = await db.Repository.GetAsync(id);
        Assert.Equal(RequestStatus.Unassigned, stored!.Status);
        Assert.Equal(0L, stored.Version);
        Assert.Empty(await db.Repository.GetHistoryAsync(id));
    }

    [Fact]
    public async Task Stale_transition_is_reported_as_conflict_and_changes_nothing()
    {
        using var db = await SqliteTestDatabase.CreateAsync();
        var id = await db.SubmitAsync(SqliteTestDatabase.Requester1);
        var record = new TransitionRecord(id, RequestStatus.Unassigned, RequestStatus.Assigned, "staff-roads",
            DateTimeOffset.UtcNow, "staff-roads-2", null, null, ExpectedVersion: 0);

        Assert.True(await db.Repository.SaveTransitionAsync(record));
        Assert.False(await db.Repository.SaveTransitionAsync(record)); // same stale record replayed

        var stored = await db.Repository.GetAsync(id);
        Assert.Equal(RequestStatus.Assigned, stored!.Status);
        Assert.Equal(1L, stored.Version);
        Assert.Single(await db.Repository.GetHistoryAsync(id));
    }
}
