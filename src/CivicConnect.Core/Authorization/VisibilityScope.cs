namespace CivicConnect.Core.Authorization;

/// <summary>
/// The authorization result for an actor. The same scope can be used for list filtering
/// and for checking an individual request.
/// </summary>
public sealed class VisibilityScope
{
    private readonly string? _ownerId;
    private readonly IReadOnlySet<string> _requestTypes;

    private VisibilityScope(
        string? ownerId,
        bool allRequests,
        IReadOnlySet<string> requestTypes,
        IReadOnlySet<AuthorizationAction> allowedActions)
    {
        _ownerId = ownerId;
        AllRequests = allRequests;
        _requestTypes = requestTypes;
        AllowedActions = allowedActions;
    }

    public bool OwnRequestsOnly => _ownerId is not null;
    public bool AllRequests { get; }
    public IReadOnlySet<AuthorizationAction> AllowedActions { get; }

    public static VisibilityScope OwnRequests(
        string ownerId,
        IReadOnlySet<AuthorizationAction> actions) =>
        new(ownerId, false, new HashSet<string>(), actions);

    public static VisibilityScope RequestTypes(
        IReadOnlySet<string> requestTypes,
        IReadOnlySet<AuthorizationAction> actions) =>
        new(null, false, requestTypes, actions);

    public static VisibilityScope All(IReadOnlySet<AuthorizationAction> actions) =>
        new(null, true, new HashSet<string>(), actions);

    public bool Allows(AuthorizationAction action) => AllowedActions.Contains(action);

    public bool CanView(RequestAccess request) =>
        AllRequests ||
        (_ownerId is not null &&
         string.Equals(_ownerId, request.RequesterId, StringComparison.Ordinal)) ||
        _requestTypes.Contains(request.RequestType);
}
