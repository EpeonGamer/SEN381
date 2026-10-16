using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace CivicConnect.Api;

/// <summary>Limits login attempts per client IP address (Security:LoginAttemptsPerMinute).</summary>
public static class RateLimits
{
    public const string Login = "login";

    public static void AddLoginRateLimit(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(Login, http =>
            {
                var perMinute = http.RequestServices.GetRequiredService<IConfiguration>()
                    .GetValue("Security:LoginAttemptsPerMinute", 10);
                return RateLimitPartition.GetFixedWindowLimiter(
                    http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions { PermitLimit = perMinute, Window = TimeSpan.FromMinutes(1) });
            });
        });
    }
}
