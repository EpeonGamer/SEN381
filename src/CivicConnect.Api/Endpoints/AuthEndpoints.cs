using System.Security.Claims;
using CivicConnect.Api.Auth;
using CivicConnect.Core.Lifecycle;
using CivicConnect.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CivicConnect.Api.Endpoints;

/// <summary>Login/logout with an HttpOnly cookie (NFR-002, NFR-005).</summary>
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth");

        auth.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .RequireRateLimiting(RateLimits.Login); // slows password guessing

        auth.MapPost("/logout", async (HttpContext http) =>
        {
            await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.NoContent();
        });

        auth.MapGet("/me", (ClaimsPrincipal user) =>
            Results.Ok(new UserView(user.ToActor().Id, user.ToActor().Role.ToString())));
    }

    private static async Task<IResult> LoginAsync(LoginBody body, HttpContext http,
        IRequestRepository repository, SqliteCredentialStore credentials, ILogger<LoginBody> logger)
    {
        var actor = string.IsNullOrWhiteSpace(body.UserId) ? null : await repository.GetActorAsync(body.UserId);
        var storedHash = actor is null ? null : await credentials.GetPasswordHashAsync(actor.Id);

        // Same answer for "unknown user" and "wrong password", so user ids cannot be discovered.
        if (!PasswordHasher.Verify(body.Password, storedHash))
        {
            logger.LogWarning("Failed login for user id {UserId}", body.UserId);
            return ApiResults.Error(StatusCodes.Status401Unauthorized, "InvalidCredentials");
        }

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, actor!.Id),
            new Claim(ClaimTypes.Role, actor.Role.ToString())
        }, CookieAuthenticationDefaults.AuthenticationScheme);

        await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        return Results.Ok(new UserView(actor.Id, actor.Role.ToString()));
    }
}
