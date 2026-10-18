using Microsoft.Data.Sqlite;

namespace CivicConnect.Data;

/// <summary>
/// Stores password hashes for login (NFR-002: no plaintext passwords). Kept in its own table so the
/// Core schema in <see cref="SqliteSchema"/> is unchanged. Hashing itself is done by the API; this class
/// only reads and writes the hash text.
/// </summary>
public sealed class SqliteCredentialStore
{
    private readonly string _connectionString;

    public SqliteCredentialStore(string connectionString) => _connectionString = connectionString;

    public async Task EnsureCreatedAsync()
    {
        await ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS user_credentials (
                user_id       TEXT PRIMARY KEY REFERENCES users(id),
                password_hash TEXT NOT NULL
            );
            """);
    }

    public async Task<string?> GetPasswordHashAsync(string userId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT password_hash FROM user_credentials WHERE user_id = $id";
        command.Parameters.AddWithValue("$id", userId);
        return await command.ExecuteScalarAsync() as string;
    }

    /// <summary>Creates or replaces the stored hash for a user.</summary>
    public Task SetPasswordHashAsync(string userId, string passwordHash) =>
        ExecuteAsync("""
            INSERT INTO user_credentials (user_id, password_hash) VALUES ($id, $hash)
            ON CONFLICT(user_id) DO UPDATE SET password_hash = excluded.password_hash
            """, ("$id", userId), ("$hash", passwordHash));

    /// <summary>Deletes the stored hash, so the user can no longer log in.</summary>
    public Task RemovePasswordHashAsync(string userId) =>
        ExecuteAsync("DELETE FROM user_credentials WHERE user_id = $id", ("$id", userId));

    private async Task ExecuteAsync(string sql, params (string Name, object Value)[] parameters)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON; " + sql;
        foreach (var (name, value) in parameters) command.Parameters.AddWithValue(name, value);
        await command.ExecuteNonQueryAsync();
    }
}
