using Microsoft.Data.Sqlite;

namespace CivicConnect.Data;

/// <summary>Read-only reference data for the API (category picker, health check).</summary>
public static class SqliteReferenceData
{
    /// <summary>The controlled category list (FR-003).</summary>
    public static async Task<IReadOnlyList<string>> ListCategoriesAsync(string connectionString)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM categories ORDER BY name";

        var categories = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) categories.Add(reader.GetString(0));
        return categories;
    }

    /// <summary>True when the database file can be opened and queried.</summary>
    public static async Task<bool> CanConnectAsync(string connectionString)
    {
        try
        {
            await using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync();
            return true;
        }
        catch (SqliteException)
        {
            return false;
        }
    }
}
