using System.Text.Json.Serialization;
using CivicConnect.Api;
using CivicConnect.Api.Endpoints;
using CivicConnect.Api.Infrastructure;
using CivicConnect.Core.Lifecycle;
using CivicConnect.Core.Requests;
using CivicConnect.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);

// ---- 1. Core services wired to SQLite (ADR-04, ADR-05, ADR-07) ----------------------------------
builder.Services.AddSingleton(sp => CivicConnectDatabase.FromConfiguration(
    sp.GetRequiredService<IConfiguration>(), sp.GetRequiredService<IHostEnvironment>()));
builder.Services.AddSingleton<IRequestRepository>(sp =>
    new SqliteRequestRepository(sp.GetRequiredService<CivicConnectDatabase>().ConnectionString,
        sp.GetRequiredService<TimeProvider>()));
builder.Services.AddSingleton(sp =>
    new SqliteCredentialStore(sp.GetRequiredService<CivicConnectDatabase>().ConnectionString));
builder.Services.AddSingleton(sp => new RequestService(sp.GetRequiredService<IRequestRepository>(),
    clock: sp.GetRequiredService<TimeProvider>()));
builder.Services.AddSingleton(sp => new TransitionService(sp.GetRequiredService<IRequestRepository>(),
    clock: sp.GetRequiredService<TimeProvider>()));

// Statuses travel as names ("Assigned"), never as numbers.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));

// ---- 2. Authentication and authorization (NFR-002, NFR-005) ---------------------------------------
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "CivicConnect.Auth";
        options.Cookie.HttpOnly = true;                         // not readable by page scripts
        options.Cookie.SameSite = SameSiteMode.Strict;          // not sent by other sites (CSRF protection)
        options.Cookie.SecurePolicy = builder.Configuration.GetValue("Security:RequireHttps", true)
            ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        // An API answers 401/403 instead of redirecting to a login page.
        options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = 401; return Task.CompletedTask; };
        options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = 403; return Task.CompletedTask; };
    });

// Secure by default: every endpoint requires a signed-in user unless it explicitly allows anonymous access.
builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
builder.Services.AddLoginRateLimit();

var app = builder.Build();

// ---- 3. Middleware pipeline -----------------------------------------------------------------------
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var (status, code) = error switch
    {
        // Malformed input such as ?limit=abc: the caller's mistake, never a server error (DEF-10).
        BadHttpRequestException bad => (bad.StatusCode, "InvalidInput"),
        // Core services throw this when the actor no longer exists or lacks access.
        UnauthorizedAccessException => (403, "NotAuthorised"),
        _ => (500, "ServerError")
    };
    context.Response.StatusCode = status;
    await context.Response.WriteAsJsonAsync(new ApiError(code));
}));

if (app.Configuration.GetValue("Security:RequireHttps", true))
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseDefaultFiles();   // serves wwwroot/index.html (the frontend) at "/"
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// ---- 4. Endpoints -----------------------------------------------------------------------------------
app.MapGet("/health", async (CivicConnectDatabase db, IHostEnvironment env) =>
{
    var databaseOk = await SqliteReferenceData.CanConnectAsync(db.ConnectionString);
    return Results.Json(new
    {
        status = databaseOk ? "Healthy" : "Unhealthy",
        version = ReleaseInfo.Version,   // identifies the deployed release candidate
        environment = env.EnvironmentName
    }, statusCode: databaseOk ? 200 : 503);
}).AllowAnonymous();

app.MapAuthEndpoints();
app.MapRequestEndpoints();
app.MapReportEndpoints();

await DatabaseStartup.InitialiseAsync(app);
app.Run();

// Lets the test project start the real app in memory (WebApplicationFactory<Program>).
public partial class Program { }
