namespace CivicConnect.Core.Authorization;

/// <summary>
/// A configurable Staff group and the request types it permits the group to access.
/// </summary>
public sealed record StaffGroup(
    string Name,
    IReadOnlySet<string> RequestTypes);
