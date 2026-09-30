namespace CivicConnect.Core.Authorization;

/// <summary>
/// Authenticated actor information needed by the authorization layer.
/// Staff groups are data/configuration; the authorization policy decides what they permit.
/// </summary>
public sealed record ActorContext(
    string Id,
    ActorRole Role,
    IReadOnlyCollection<StaffGroup>? Groups = null)
{
    public IReadOnlyCollection<StaffGroup> StaffGroups => Groups ?? Array.Empty<StaffGroup>();
}
