using Microsoft.Data.Sqlite;

namespace CivicConnect.Api.Infrastructure;

/// <summary>
/// The resolved SQLite connection string for the current environment (ADR-07).
/// Each environment (Development, Staging, tests) uses its own database file from configuration.
/// </summary>
public sealed record CivicConnectDatabase(string ConnectionString)
{
    public static CivicConnectDatabase FromConfiguration(IConfiguration configuration, IHostEnvironment environment)
    {
        var configured = configuration.GetConnectionString("CivicConnect")
            ?? throw new InvalidOperationException("ConnectionStrings:CivicConnect is not configured.");

        // Relative paths are resolved against the app folder, and the folder is created if needed.
        var builder = new SqliteConnectionStringBuilder(configured);
        if (!Path.IsPathRooted(builder.DataSource))
            builder.DataSource = Path.Combine(environment.ContentRootPath, builder.DataSource);
        Directory.CreateDirectory(Path.GetDirectoryName(builder.DataSource)!);

        return new CivicConnectDatabase(builder.ToString());
    }
}
