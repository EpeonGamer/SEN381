using CivicConnect.Api.Auth;
using CivicConnect.Data;

namespace CivicConnect.Api.Infrastructure;

/// <summary>
/// Runs once at startup: creates the schema and, when Seed:Enabled is true (Development/Staging only),
/// adds the demo reference data and accounts. User administration is out of M3 scope (TD-01).
/// </summary>
public static class DatabaseStartup
{
    public const int MinimumPasswordLength = 8;

    public static readonly string[] Categories = { "Roads", "Water", "Waste", "General" };

    // (user id, role, staff group or null)
    public static readonly (string Id, string Role, string? Group)[] DemoUsers =
    {
        ("requester-1", "Requester", null),
        ("requester-2", "Requester", null),
        ("staff-roads", "Staff", "Roads-Team"),
        ("staff-roads-2", "Staff", "Roads-Team"),
        ("staff-utilities", "Staff", "Utilities-Team"),
        ("manager-1", "Manager", null),
    };

    public static async Task InitialiseAsync(WebApplication app)
    {
        var database = app.Services.GetRequiredService<CivicConnectDatabase>();
        var credentials = app.Services.GetRequiredService<SqliteCredentialStore>();
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseStartup");

        await SqliteSchema.EnsureCreatedAsync(database.ConnectionString);
        await credentials.EnsureCreatedAsync();

        if (!app.Configuration.GetValue<bool>("Seed:Enabled"))
            return;

        await SeedReferenceDataAsync(database.ConnectionString);

        // The password comes from configuration (e.g. the Seed__DemoPassword environment variable),
        // never from a committed file. Without it the accounts exist but nobody can log in.
        var demoPassword = app.Configuration["Seed:DemoPassword"];
        if (string.IsNullOrEmpty(demoPassword) || demoPassword.Length < MinimumPasswordLength)
        {
            logger.LogWarning("Seed:DemoPassword is not set (min {Length} chars); demo accounts cannot log in.",
                MinimumPasswordLength);
            return;
        }

        foreach (var user in DemoUsers)
            await credentials.SetPasswordHashAsync(user.Id, PasswordHasher.Hash(demoPassword));
        logger.LogInformation("Seeded {Count} demo accounts.", DemoUsers.Length);
    }

    private static async Task SeedReferenceDataAsync(string connectionString)
    {
        foreach (var category in Categories)
            await SqliteSetup.AddCategoryAsync(connectionString, category);

        await SqliteSetup.AddStaffGroupAsync(connectionString, "Roads-Team", "Roads");
        await SqliteSetup.AddStaffGroupAsync(connectionString, "Utilities-Team", "Water", "Waste");

        foreach (var (id, role, group) in DemoUsers)
        {
            await SqliteSetup.AddUserAsync(connectionString, id, role);
            if (group is not null)
                await SqliteSetup.AddUserToGroupAsync(connectionString, id, group);
        }
    }
}
