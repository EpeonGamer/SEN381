using CivicConnect.Core.Lifecycle;
using CivicConnect.Core.Requests;
using CivicConnect.Data;
using Microsoft.Data.Sqlite;
using Xunit;

namespace CivicConnect.Core.Tests;

public class ReassignmentTests
{
    [Fact]
    public async Task Utilities_capability_uses_group_membership_and_preserves_assignment_history()
    {
        using var db = await SqliteTestDatabase.CreateAsync();
        await SqliteSetup.AddUserAsync(db.ConnectionString, "utility-a", "Staff");
        await SqliteSetup.AddUserToGroupAsync(db.ConnectionString, "utility-a", "General-Team");
        await SqliteSetup.AddUserAsync(db.ConnectionString, "utility-b", "Staff");
        await SqliteSetup.AddUserToGroupAsync(db.ConnectionString, "utility-b", "General-Team");
        var actor = (await db.Repository.GetActorAsync("utility-a"))!;
        var id = await db.SubmitAsync(SqliteTestDatabase.Requester1, "General");
        Assert.True((await db.Transitions.ApplyAsync(new TransitionRequest(id, RequestStatus.Assigned, actor, "utility-a"))).Succeeded);
        Assert.True((await db.Requests.ReassignAsync(actor, id, "utility-b", expectedVersion: 1)).Succeeded);
        Assert.Equal(RequestFailure.ConcurrencyConflict, (await db.Requests.ReassignAsync(actor, id, "utility-a", expectedVersion: 1)).Failure);
        var stored = (await db.Repository.GetAsync(id))!;
        Assert.Equal("utility-b", stored.AssigneeId);
        Assert.Equal(2, stored.Version);
        Assert.False(await db.Repository.ReassignAsync(stored with { Version = 1 }, "utility-a", actor.Id, DateTimeOffset.UtcNow));
        await using var connection = new SqliteConnection(db.ConnectionString);
        await connection.OpenAsync();
        await using var query = connection.CreateCommand();
        query.CommandText = "SELECT assignee_id, assigned_by, ended_at FROM assignment_history WHERE request_id = $id ORDER BY id";
        query.Parameters.AddWithValue("$id", id.ToString());
        await using var reader = await query.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.Equal("utility-a", reader.GetString(0));
        Assert.False(reader.IsDBNull(2));
        Assert.True(await reader.ReadAsync());
        Assert.Equal("utility-b", reader.GetString(0));
        Assert.Equal("utility-a", reader.GetString(1));
        Assert.True(reader.IsDBNull(2));
        Assert.False(await reader.ReadAsync());
    }

    [Fact]
    public async Task Failed_assignment_audit_rolls_back_responsibility_and_version()
    {
        using var db = await SqliteTestDatabase.CreateAsync();
        var id = await db.SubmitAsync(SqliteTestDatabase.Requester1);
        await db.Transitions.ApplyAsync(new TransitionRequest(id, RequestStatus.Assigned, SqliteTestDatabase.RoadsStaff, "staff-roads"));
        var before = (await db.Repository.GetAsync(id))!;
        await using var connection = new SqliteConnection(db.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "CREATE TRIGGER fail_assignment BEFORE INSERT ON assignment_history BEGIN SELECT RAISE(ABORT, 'injected failure'); END;";
        await command.ExecuteNonQueryAsync();
        await Assert.ThrowsAsync<SqliteException>(() => db.Requests.ReassignAsync(SqliteTestDatabase.RoadsStaff, id, "staff-roads-2"));
        Assert.Equal(before, await db.Repository.GetAsync(id));
    }

    [Fact]
    public async Task Terminal_request_and_same_assignee_are_rejected()
    {
        using var db = await SqliteTestDatabase.CreateAsync();
        var id = await db.SubmitAsync(SqliteTestDatabase.Requester1);
        await db.Transitions.ApplyAsync(new TransitionRequest(id, RequestStatus.Assigned, SqliteTestDatabase.RoadsStaff, "staff-roads"));
        Assert.Equal(RequestFailure.InvalidInput, (await db.Requests.ReassignAsync(SqliteTestDatabase.RoadsStaff, id, "staff-roads")).Failure);
        await db.Transitions.ApplyAsync(new TransitionRequest(id, RequestStatus.Resolved, SqliteTestDatabase.RoadsStaff, ResolutionNote: "Done"));
        Assert.Equal(RequestFailure.InvalidState, (await db.Requests.ReassignAsync(SqliteTestDatabase.RoadsStaff, id, "staff-roads-2")).Failure);
    }
}
