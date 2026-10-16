using Microsoft.Data.Sqlite;

namespace CivicConnect.Data;

/// <summary>
/// Reference-data helpers for creating a staging/test database and seeding users, categories and
/// staff groups. Not part of the Core persistence interface (user management is out of scope for M3).
/// </summary>
public static class SqliteSetup
{
    public static Task AddCategoryAsync(string connectionString, string category) =>
        ExecuteAsync(connectionString, "INSERT OR IGNORE INTO categories (name) VALUES ($a)", ("$a", category));

    public static Task AddUserAsync(string connectionString, string id, string role) =>
        ExecuteAsync(connectionString, "INSERT OR IGNORE INTO users (id, role) VALUES ($a, $b)", ("$a", id), ("$b", role));

    public static async Task AddStaffGroupAsync(string connectionString, string group, params string[] categories)
    {
        await ExecuteAsync(connectionString, "INSERT OR IGNORE INTO staff_groups (name) VALUES ($a)", ("$a", group));
        foreach (var category in categories)
            await ExecuteAsync(connectionString,
                "INSERT OR IGNORE INTO staff_group_categories (group_name, category) VALUES ($a, $b)", ("$a", group), ("$b", category));
    }

    public static Task AddUserToGroupAsync(string connectionString, string userId, string group) =>
        ExecuteAsync(connectionString, "INSERT OR IGNORE INTO user_staff_groups (user_id, group_name) VALUES ($a, $b)", ("$a", userId), ("$b", group));

    private static async Task ExecuteAsync(string connectionString, string sql, params (string Name, object Value)[] parameters)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        await using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        await pragma.ExecuteNonQueryAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (var (name, value) in parameters) command.Parameters.AddWithValue(name, value);
        await command.ExecuteNonQueryAsync();
    }
}
