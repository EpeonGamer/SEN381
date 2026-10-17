using System.Security.Cryptography;
using System.Text.Json;
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

    private static readonly JsonSerializerOptions IndentedJson = new() { WriteIndented = true };

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

        // Every demo account has its own password (TD-06). The passwords live in a JSON file outside source
        // control (Seed:PasswordsFile, e.g. a git-ignored data folder); an account without one gets a new random
        // password, so adding a user to DemoUsers is enough. Only hashes are stored in the database (NFR-002).
        var passwordsFile = app.Configuration["Seed:PasswordsFile"];
        if (string.IsNullOrWhiteSpace(passwordsFile))
        {
            logger.LogWarning("Seed:PasswordsFile is not set; demo accounts cannot log in.");
            return;
        }

        passwordsFile = Path.GetFullPath(passwordsFile, app.Environment.ContentRootPath);
        var passwords = await LoadOrCreatePasswordsAsync(passwordsFile);

        var seeded = 0;
        foreach (var user in DemoUsers)
        {
            var password = passwords[user.Id] ?? "";
            var sharedWith = DemoUsers.Where(other => other.Id != user.Id && passwords[other.Id] == password)
                                      .Select(other => other.Id).ToArray();
            if (password.Length < MinimumPasswordLength || sharedWith.Length > 0)
            {
                // Refuse rather than fall back to a weak or shared password; removing the hash also locks out
                // an old password left by an earlier seed.
                await credentials.RemovePasswordHashAsync(user.Id);
                logger.LogWarning("Demo account {User} cannot log in: its password must be at least {Length} characters " +
                                  "and not shared with another account ({Shared}).",
                    user.Id, MinimumPasswordLength, string.Join(", ", sharedWith));
                continue;
            }

            await credentials.SetPasswordHashAsync(user.Id, PasswordHasher.Hash(password));
            seeded++;
        }
        logger.LogInformation("Seeded {Count} demo accounts; their passwords are in {File}.", seeded, passwordsFile);
    }

    /// <summary>Reads the user-id-to-password map, adding a random password for any demo account not in it yet.</summary>
    private static async Task<Dictionary<string, string>> LoadOrCreatePasswordsAsync(string path)
    {
        var passwords = File.Exists(path)
            ? JsonSerializer.Deserialize<Dictionary<string, string>>(await File.ReadAllTextAsync(path)) ?? new()
            : new Dictionary<string, string>();

        var missing = DemoUsers.Select(user => user.Id).Where(id => !passwords.ContainsKey(id)).ToArray();
        if (missing.Length == 0)
            return passwords;

        foreach (var id in missing)
            passwords[id] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(12)) // 16 characters
                                 .Replace('+', '-').Replace('/', '_');

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(passwords, IndentedJson));
        return passwords;
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
