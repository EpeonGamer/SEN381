using System.Reflection;

namespace CivicConnect.Api.Infrastructure;

/// <summary>Version plus Git commit, e.g. "0.3.0+950f885...", stamped into the build by the .NET SDK.</summary>
public static class ReleaseInfo
{
    public static string Version { get; } =
        typeof(ReleaseInfo).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "unknown";
}
