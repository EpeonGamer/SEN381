using System.Data.Common;
using System.Globalization;
using System.Text;
using CivicConnect.Core.Authorization;
using CivicConnect.Core.Lifecycle;
using CivicConnect.Core.Requests;
using Microsoft.Data.Sqlite;

namespace CivicConnect.Data;

/// <summary>
/// SQLite implementation of <see cref="IRequestRepository"/> (ADR-07).
/// Each call uses its own connection. Visibility filtering is applied in SQL before rows are
/// returned (ADR-05, NFR-007). Status changes and their audit entries are written in a single
/// transaction guarded by a status + version check (NFR-004, DEC-M2-03).
/// </summary>
public sealed class SqliteRequestRepository : IRequestRepository
{
    private const string RequestSelect =
        "SELECT id, requester_id, category, title, description, status, submitted_at, updated_at, assignee_id, version FROM requests";

    private readonly string _connectionString;
    private readonly TimeProvider _clock;

    public SqliteRequestRepository(string connectionString, TimeProvider? clock = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("A connection string is required.", nameof(connectionString));
        _connectionString = connectionString;
        _clock = clock ?? TimeProvider.System;
    }

    public async Task<ServiceRequest?> GetAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = RequestSelect + " WHERE id = $id";
        command.Parameters.AddWithValue("$id", requestId.ToString());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapRequest(reader) : null;
    }

    public async Task<ActorContext?> GetActorAsync(string actorId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);

        await using var roleCommand = connection.CreateCommand();
        roleCommand.CommandText = "SELECT role FROM users WHERE id = $id";
        roleCommand.Parameters.AddWithValue("$id", actorId);
        var roleText = await roleCommand.ExecuteScalarAsync(cancellationToken) as string;
        if (roleText is null) return null;
        var role = Enum.Parse<ActorRole>(roleText);

        await using var groupCommand = connection.CreateCommand();
        groupCommand.CommandText = """
            SELECT usg.group_name, sgc.category
            FROM user_staff_groups usg
            LEFT JOIN staff_group_categories sgc ON sgc.group_name = usg.group_name
            WHERE usg.user_id = $id
            ORDER BY usg.group_name
            """;
        groupCommand.Parameters.AddWithValue("$id", actorId);

        var groups = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        await using var reader = await groupCommand.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var name = reader.GetString(0);
            if (!groups.TryGetValue(name, out var types))
                groups[name] = types = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!reader.IsDBNull(1)) types.Add(reader.GetString(1));
        }

        return new ActorContext(actorId, role,
            groups.Select(g => new StaffGroup(g.Key, g.Value)).ToList());
    }

    public async Task<bool> CategoryExistsAsync(string category, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1 FROM categories WHERE name = $name"; // column is NOCASE
        command.Parameters.AddWithValue("$name", category);
        return await command.ExecuteScalarAsync(cancellationToken) is not null;
    }

    public async Task CreateAsync(ServiceRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO requests (id, requester_id, category, title, description, status, submitted_at, updated_at, assignee_id, version)
            VALUES ($id, $requester, $category, $title, $description, $status, $submitted, $updated, $assignee, $version)
            """;
        command.Parameters.AddWithValue("$id", request.Id.ToString());
        command.Parameters.AddWithValue("$requester", request.RequesterId);
        command.Parameters.AddWithValue("$category", request.Category);
        command.Parameters.AddWithValue("$title", request.Title);
        command.Parameters.AddWithValue("$description", request.Description);
        command.Parameters.AddWithValue("$status", (int)request.Status);
        command.Parameters.AddWithValue("$submitted", Format(request.SubmittedAt));
        command.Parameters.AddWithValue("$updated", Format(request.UpdatedAt));
        command.Parameters.AddWithValue("$assignee", (object?)request.AssigneeId ?? DBNull.Value);
        command.Parameters.AddWithValue("$version", request.Version);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceRequest>> ListAsync(VisibilityScope scope, RequestQuery query,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        var sql = new StringBuilder(RequestSelect);
        AppendFilters(command, sql, scope, query, _clock.GetUtcNow());
        sql.Append(query.SortByCategory
            ? " ORDER BY category COLLATE NOCASE, submitted_at DESC, id"
            : " ORDER BY submitted_at DESC, id");
        sql.Append(" LIMIT $limit OFFSET $offset");
        command.Parameters.AddWithValue("$limit", query.Limit);
        command.Parameters.AddWithValue("$offset", query.Offset);
        command.CommandText = sql.ToString();

        var results = new List<ServiceRequest>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) results.Add(MapRequest(reader));
        return results;
    }

    public async Task<IReadOnlyList<TransitionRecord>> GetHistoryAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT request_id, from_status, to_status, actor_id, at, assignee_id, reason, resolution_note, version_before
            FROM transition_history WHERE request_id = $id ORDER BY id
            """;
        command.Parameters.AddWithValue("$id", requestId.ToString());
        var results = new List<TransitionRecord>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            results.Add(new TransitionRecord(
                Guid.Parse(reader.GetString(0)), (RequestStatus)reader.GetInt32(1), (RequestStatus)reader.GetInt32(2),
                reader.GetString(3), Parse(reader.GetString(4)),
                reader.IsDBNull(5) ? null : reader.GetString(5),
                reader.IsDBNull(6) ? null : reader.GetString(6),
                reader.IsDBNull(7) ? null : reader.GetString(7),
                reader.GetInt64(8)));
        return results;
    }

    public async Task<IReadOnlyList<RequestComment>> GetCommentsAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT request_id, author_id, content, at FROM request_comments WHERE request_id = $id ORDER BY id";
        command.Parameters.AddWithValue("$id", requestId.ToString());
        var results = new List<RequestComment>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            results.Add(new RequestComment(Guid.Parse(reader.GetString(0)), reader.GetString(1),
                reader.GetString(2), Parse(reader.GetString(3))));
        return results;
    }

    public async Task AddCommentAsync(RequestComment comment, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO request_comments (request_id, author_id, content, at) VALUES ($r, $a, $c, $t)";
        command.Parameters.AddWithValue("$r", comment.RequestId.ToString());
        command.Parameters.AddWithValue("$a", comment.AuthorId);
        command.Parameters.AddWithValue("$c", comment.Content);
        command.Parameters.AddWithValue("$t", Format(comment.At));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> ReassignAsync(ServiceRequest expected, string assigneeId, string actorId, DateTimeOffset at,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

        await using (var update = connection.CreateCommand())
        {
            update.Transaction = transaction;
            update.CommandText = """
                UPDATE requests
                SET assignee_id = $assignee, updated_at = $at, version = version + 1
                WHERE id = $id AND status = $assigned AND version = $version
                """;
            update.Parameters.AddWithValue("$assignee", assigneeId);
            update.Parameters.AddWithValue("$at", Format(at));
            update.Parameters.AddWithValue("$id", expected.Id.ToString());
            update.Parameters.AddWithValue("$assigned", (int)RequestStatus.Assigned);
            update.Parameters.AddWithValue("$version", expected.Version);
            if (await update.ExecuteNonQueryAsync(cancellationToken) == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }
        }

        await RecordAssignmentAsync(connection, transaction, expected.Id, assigneeId, actorId, at, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<ActivityCounts> GetCountsAsync(VisibilityScope scope, RequestQuery query,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        // Overdue remains a subset of Open; use the same instant for counting and filtering.
        var now = _clock.GetUtcNow();
        var sql = new StringBuilder("""
            SELECT
              COALESCE(SUM(CASE WHEN status IN (0, 1) THEN 1 ELSE 0 END), 0),
              COALESCE(SUM(CASE WHEN status = 2 THEN 1 ELSE 0 END), 0),
              COALESCE(SUM(CASE WHEN status = 3 THEN 1 ELSE 0 END), 0),
              COALESCE(SUM(CASE WHEN status IN (0, 1) AND submitted_at < $overdueCutoff THEN 1 ELSE 0 END), 0)
            FROM requests
            """);
        command.Parameters.AddWithValue("$overdueCutoff", Format(OverduePolicy.Cutoff(now)));
        AppendFilters(command, sql, scope, query, now);
        command.CommandText = sql.ToString();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return new ActivityCounts(reader.GetInt64(0), reader.GetInt64(1), reader.GetInt64(2), reader.GetInt64(3));
    }

    public async Task<bool> SaveTransitionAsync(TransitionRecord record, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        // Any exception below disposes the transaction without commit, which rolls back both writes.
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

        await using (var update = connection.CreateCommand())
        {
            update.Transaction = transaction;
            update.CommandText = """
                UPDATE requests
                SET status = $to,
                    updated_at = $at,
                    version = version + 1,
                    assignee_id = CASE WHEN $setAssignee = 1 THEN $assignee ELSE assignee_id END
                WHERE id = $id AND status = $from AND version = $version
                """;
            update.Parameters.AddWithValue("$to", (int)record.To);
            update.Parameters.AddWithValue("$at", Format(record.At));
            update.Parameters.AddWithValue("$setAssignee", record.AssigneeId is null ? 0 : 1);
            update.Parameters.AddWithValue("$assignee", (object?)record.AssigneeId ?? DBNull.Value);
            update.Parameters.AddWithValue("$id", record.RequestId.ToString());
            update.Parameters.AddWithValue("$from", (int)record.From);
            update.Parameters.AddWithValue("$version", record.ExpectedVersion);
            if (await update.ExecuteNonQueryAsync(cancellationToken) == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return false; // stale status or version: someone else changed it first
            }
        }

        await using (var history = connection.CreateCommand())
        {
            history.Transaction = transaction;
            history.CommandText = """
                INSERT INTO transition_history
                    (request_id, from_status, to_status, actor_id, at, assignee_id, reason, resolution_note, version_before)
                VALUES ($id, $from, $to, $actor, $at, $assignee, $reason, $note, $version)
                """;
            history.Parameters.AddWithValue("$id", record.RequestId.ToString());
            history.Parameters.AddWithValue("$from", (int)record.From);
            history.Parameters.AddWithValue("$to", (int)record.To);
            history.Parameters.AddWithValue("$actor", record.ActorId);
            history.Parameters.AddWithValue("$at", Format(record.At));
            history.Parameters.AddWithValue("$assignee", (object?)record.AssigneeId ?? DBNull.Value);
            history.Parameters.AddWithValue("$reason", (object?)record.Reason ?? DBNull.Value);
            history.Parameters.AddWithValue("$note", (object?)record.ResolutionNote ?? DBNull.Value);
            history.Parameters.AddWithValue("$version", record.ExpectedVersion);
            await history.ExecuteNonQueryAsync(cancellationToken);
        }

        if (record.To == RequestStatus.Assigned && record.AssigneeId is not null)
            await RecordAssignmentAsync(connection, transaction, record.RequestId, record.AssigneeId,
                record.ActorId, record.At, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    // ---- helpers ----

    private async Task<SqliteConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        await pragma.ExecuteNonQueryAsync(cancellationToken);
        return connection;
    }

    private static async Task RecordAssignmentAsync(SqliteConnection connection, SqliteTransaction transaction,
        Guid requestId, string assigneeId, string actorId, DateTimeOffset at, CancellationToken cancellationToken)
    {
        await using (var close = connection.CreateCommand())
        {
            close.Transaction = transaction;
            close.CommandText = "UPDATE assignment_history SET ended_at = $at WHERE request_id = $id AND ended_at IS NULL";
            close.Parameters.AddWithValue("$at", Format(at));
            close.Parameters.AddWithValue("$id", requestId.ToString());
            await close.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var insert = connection.CreateCommand();
        insert.Transaction = transaction;
        insert.CommandText = """
            INSERT INTO assignment_history (request_id, assignee_id, assigned_by, assigned_at)
            VALUES ($id, $assignee, $by, $at)
            """;
        insert.Parameters.AddWithValue("$id", requestId.ToString());
        insert.Parameters.AddWithValue("$assignee", assigneeId);
        insert.Parameters.AddWithValue("$by", actorId);
        insert.Parameters.AddWithValue("$at", Format(at));
        await insert.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>Mirrors <see cref="VisibilityScope.CanView"/>: all, OR owner, OR permitted category. An empty scope returns nothing.</summary>
    private static void AppendFilters(SqliteCommand command, StringBuilder sql, VisibilityScope scope, RequestQuery query,
        DateTimeOffset now)
    {
        sql.Append(" WHERE 1 = 1");

        if (!scope.AllRequests)
        {
            var parts = new List<string>();
            if (scope.OwnRequestsOnly)
            {
                parts.Add("requester_id = $owner");
                command.Parameters.AddWithValue("$owner", scope.OwnerId);
            }
            if (scope.PermittedRequestTypes.Count > 0)
            {
                var names = new List<string>();
                var i = 0;
                foreach (var type in scope.PermittedRequestTypes)
                {
                    var name = $"$type{i++}";
                    names.Add(name);
                    command.Parameters.AddWithValue(name, type);
                }
                parts.Add($"category IN ({string.Join(", ", names)})"); // column is NOCASE
            }
            sql.Append(parts.Count == 0 ? " AND 1 = 0" : $" AND ({string.Join(" OR ", parts)})");
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            sql.Append("""
         AND (
             instr(lower(id), lower($search)) > 0
             OR instr(lower(title), lower($search)) > 0
             OR instr(lower(description), lower($search)) > 0
             OR instr(lower(coalesce(assignee_id, '')), lower($search)) > 0
         )
        """);

            command.Parameters.AddWithValue("$search", query.Search.Trim());
        }

        if (query.Id is { } id)
        {
            sql.Append(" AND id = $queryId");
            command.Parameters.AddWithValue("$queryId", id.ToString());
        }
        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            sql.Append(" AND category = $queryCategory");
            command.Parameters.AddWithValue("$queryCategory", query.Category.Trim());
        }
        if (query.Status is { } status)
        {
            sql.Append(" AND status = $queryStatus");
            command.Parameters.AddWithValue("$queryStatus", (int)status);
        }
        if (query.Overdue is { } overdue)
        {
            sql.Append(overdue
                ? " AND (status IN (0, 1) AND submitted_at < $filterOverdueCutoff)"
                : " AND NOT (status IN (0, 1) AND submitted_at < $filterOverdueCutoff)");
            command.Parameters.AddWithValue("$filterOverdueCutoff", Format(OverduePolicy.Cutoff(now)));
        }
    }

    private static ServiceRequest MapRequest(DbDataReader r) => new(
        Guid.Parse(r.GetString(0)), r.GetString(1), r.GetString(2), r.GetString(3), r.GetString(4),
        (RequestStatus)r.GetInt32(5), Parse(r.GetString(6)), Parse(r.GetString(7)),
        r.IsDBNull(8) ? null : r.GetString(8), r.GetInt64(9));

    private static string Format(DateTimeOffset value) =>
        value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

    private static DateTimeOffset Parse(string value) =>
        DateTimeOffset.Parse(value, CultureInfo.InvariantCulture);
}
