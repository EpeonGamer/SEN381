using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CivicConnect.Api.Tests;

/// <summary>
/// The system under test, reached over HTTP. Two modes:
/// <list type="bullet">
/// <item>In-process (default): the real Program, real middleware and a real temporary SQLite file.</item>
/// <item>Deployed: when CIVICCONNECT_E2E_BASEURL is set (e.g. the staging URL), tests call that server over the
/// network, logging in with the per-account passwords in the file named by CIVICCONNECT_E2E_PASSWORDS_FILE
/// (the staging Seed:PasswordsFile). Only the E2E tests use this mode.</item>
/// </list>
/// </summary>
public sealed class ApiTarget : IDisposable
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly WebApplicationFactory<Program>? _factory;
    private readonly Uri? _deployedBaseUrl;
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"civicconnect-api-test-{Guid.NewGuid():N}.db");
    private readonly string _passwordsFile;
    private Dictionary<string, string>? _passwords;

    public bool IsDeployed => _deployedBaseUrl is not null;
    internal string ConnectionString => $"Data Source={_databasePath}";

    private ApiTarget(Uri? deployedBaseUrl, string? deployedPasswordsFile, TimeProvider? clock = null, string environment = "Testing",
        IReadOnlyDictionary<string, string>? seedPasswords = null)
    {
        _deployedBaseUrl = deployedBaseUrl;
        if (IsDeployed)
        {
            _passwordsFile = deployedPasswordsFile ?? throw new InvalidOperationException("Set CIVICCONNECT_E2E_PASSWORDS_FILE.");
            return;
        }

        // The app generates a fresh password per account into this file at startup, unless the test supplies some.
        _passwordsFile = Path.ChangeExtension(_databasePath, ".passwords.json");
        if (seedPasswords is not null)
            File.WriteAllText(_passwordsFile, JsonSerializer.Serialize(seedPasswords));
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(host =>
        {
            host.UseEnvironment(environment);
            if (clock is not null)
                host.ConfigureServices(services => services.AddSingleton<TimeProvider>(clock));
            host.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:CivicConnect"] = $"Data Source={_databasePath}",
                ["Seed:Enabled"] = "true",
                ["Seed:PasswordsFile"] = _passwordsFile,
                ["Security:RequireHttps"] = "false",       // the in-memory test server speaks plain HTTP
                ["Security:LoginAttemptsPerMinute"] = "1000"
            }));
        });
    }

    /// <summary>Always in-process: used by the API integration tests, which need a fresh database.</summary>
    /// <param name="environment">ASP.NET Core environment; "Development" reproduces developer-only behaviour (DEF-10).</param>
    /// <param name="seedPasswords">Pre-existing contents of the seed passwords file; accounts not listed get generated ones.</param>
    public static ApiTarget InProcess(TimeProvider? clock = null, string environment = "Testing",
        IReadOnlyDictionary<string, string>? seedPasswords = null) =>
        new(null, null, clock, environment, seedPasswords);

    /// <summary>Deployed environment when configured, otherwise in-process. Used by the E2E tests.</summary>
    public static ApiTarget FromEnvironment()
    {
        var url = Environment.GetEnvironmentVariable("CIVICCONNECT_E2E_BASEURL");
        return string.IsNullOrWhiteSpace(url)
            ? InProcess()
            : new ApiTarget(new Uri(url), Environment.GetEnvironmentVariable("CIVICCONNECT_E2E_PASSWORDS_FILE"));
    }

    /// <summary>A client with its own cookie jar and no login.</summary>
    public HttpClient Anonymous()
    {
        if (_factory is not null)
            return _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

        return new HttpClient(new HttpClientHandler { CookieContainer = new CookieContainer() })
        {
            BaseAddress = _deployedBaseUrl
        };
    }

    /// <summary>A client logged in as one of the seeded demo accounts.</summary>
    public async Task<HttpClient> LoginAsAsync(string userId)
    {
        var client = Anonymous();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { userId, password = PasswordOf(userId) });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return client;
    }

    /// <summary>The seeded password of a demo account, read from the seed passwords file (after the server started).</summary>
    public string PasswordOf(string userId)
    {
        if (_factory is not null)
            _ = _factory.Server; // make sure startup seeding has written the file
        _passwords ??= JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(_passwordsFile))!;
        return _passwords[userId];
    }

    public void Dispose()
    {
        _factory?.Dispose();
        SqliteConnection.ClearAllPools();
        try { File.Delete(_databasePath); } catch (IOException) { /* temp file; ignore */ }
        if (!IsDeployed)
            File.Delete(_passwordsFile);
    }
}

/// <summary>Small helpers so test bodies read like the scenario they describe.</summary>
public static class HttpTestExtensions
{
    public static Task<HttpResponseMessage> PostJsonAsync(this HttpClient client, string url, object body) =>
        client.PostAsJsonAsync(url, body, ApiTarget.Json);

    public static async Task<T> ReadAsync<T>(this HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<T>(ApiTarget.Json))!;
}
