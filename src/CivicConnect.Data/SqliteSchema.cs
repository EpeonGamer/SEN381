using Microsoft.Data.Sqlite;

namespace CivicConnect.Data;

/// <summary>
/// Database schema for the SQLite persistence adapter (ADR-07, which revises DEC-M2-01/DEC-M2-06 for M3).
/// The schema implements the M2 data baseline: current state plus history (DEC-M2-02),
/// optimistic versioning for conflict handling (DEC-M2-03) and ownership/category columns used by
/// the visibility policy (ADR-05).
/// </summary>
public static class SqliteSchema
{
    public const int Version = 1;

    private const string Sql = """
        CREATE TABLE IF NOT EXISTS users (
            id   TEXT PRIMARY KEY,
            role TEXT NOT NULL CHECK (role IN ('Requester', 'Staff', 'Manager'))
        );

        CREATE TABLE IF NOT EXISTS categories (
            name TEXT PRIMARY KEY COLLATE NOCASE
        );

        CREATE TABLE IF NOT EXISTS staff_groups (
            name TEXT PRIMARY KEY
        );

        CREATE TABLE IF NOT EXISTS staff_group_categories (
            group_name TEXT NOT NULL REFERENCES staff_groups(name),
            category   TEXT NOT NULL COLLATE NOCASE REFERENCES categories(name),
            PRIMARY KEY (group_name, category)
        );

        CREATE TABLE IF NOT EXISTS user_staff_groups (
            user_id    TEXT NOT NULL REFERENCES users(id),
            group_name TEXT NOT NULL REFERENCES staff_groups(name),
            PRIMARY KEY (user_id, group_name)
        );

        CREATE TABLE IF NOT EXISTS requests (
            id           TEXT PRIMARY KEY,
            requester_id TEXT NOT NULL REFERENCES users(id),
            category     TEXT NOT NULL COLLATE NOCASE REFERENCES categories(name),
            title        TEXT NOT NULL CHECK (length(trim(title)) > 0),
            description  TEXT NOT NULL CHECK (length(trim(description)) > 0),
            status       INTEGER NOT NULL DEFAULT 0 CHECK (status IN (0, 1, 2, 3)),
            submitted_at TEXT NOT NULL,
            updated_at   TEXT NOT NULL,
            assignee_id  TEXT NULL REFERENCES users(id),
            version      INTEGER NOT NULL DEFAULT 0
        );

        CREATE TABLE IF NOT EXISTS transition_history (
            id              INTEGER PRIMARY KEY AUTOINCREMENT,
            request_id      TEXT NOT NULL REFERENCES requests(id),
            from_status     INTEGER NOT NULL,
            to_status       INTEGER NOT NULL,
            actor_id        TEXT NOT NULL REFERENCES users(id),
            at              TEXT NOT NULL,
            assignee_id     TEXT NULL,
            reason          TEXT NULL,
            resolution_note TEXT NULL,
            version_before  INTEGER NOT NULL
        );

        CREATE TABLE IF NOT EXISTS assignment_history (
            id          INTEGER PRIMARY KEY AUTOINCREMENT,
            request_id  TEXT NOT NULL REFERENCES requests(id),
            assignee_id TEXT NOT NULL REFERENCES users(id),
            assigned_by TEXT NOT NULL REFERENCES users(id),
            assigned_at TEXT NOT NULL,
            ended_at    TEXT NULL
        );

        CREATE TABLE IF NOT EXISTS request_comments (
            id         INTEGER PRIMARY KEY AUTOINCREMENT,
            request_id TEXT NOT NULL REFERENCES requests(id),
            author_id  TEXT NOT NULL REFERENCES users(id),
            content    TEXT NOT NULL CHECK (length(trim(content)) > 0),
            at         TEXT NOT NULL
        );

        CREATE INDEX IF NOT EXISTS ix_requests_requester ON requests(requester_id);
        CREATE INDEX IF NOT EXISTS ix_requests_category_status ON requests(category, status);
        CREATE INDEX IF NOT EXISTS ix_requests_submitted ON requests(submitted_at);
        CREATE INDEX IF NOT EXISTS ix_transition_request ON transition_history(request_id);
        CREATE INDEX IF NOT EXISTS ix_assignment_request ON assignment_history(request_id);
        CREATE INDEX IF NOT EXISTS ix_comments_request ON request_comments(request_id);
        """;

    /// <summary>Creates the schema if it does not exist. Safe to call repeatedly.</summary>
    public static async Task EnsureCreatedAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        // WAL journal mode (stored in the file) lets readers continue while a write is in progress, which
        // reduced queued-write latency under concurrent load (DEF-09). Writes are still one at a time.
        command.CommandText = "PRAGMA journal_mode = WAL;\n" + Sql + $"\nPRAGMA user_version = {Version};";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
