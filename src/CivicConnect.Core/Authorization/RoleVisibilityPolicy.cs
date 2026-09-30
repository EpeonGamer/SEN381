namespace CivicConnect.Core.Authorization;

/// <summary>
/// Strategy implementation for CivicConnect's current role-based visibility rules.
/// Staff visibility is the union of all request types granted by the actor's groups.
/// </summary>
public sealed class RoleVisibilityPolicy : IVisibilityPolicy
{
    private static readonly IReadOnlySet<AuthorizationAction> RequesterActions =
        new HashSet<AuthorizationAction> { AuthorizationAction.ViewRequest };

    private static readonly IReadOnlySet<AuthorizationAction> StaffActions =
        new HashSet<AuthorizationAction>
        {
            AuthorizationAction.ViewRequest,
            AuthorizationAction.ChangeStatus
        };

    private static readonly IReadOnlySet<AuthorizationAction> ManagerActions =
        new HashSet<AuthorizationAction> { AuthorizationAction.ViewRequest };

    public VisibilityScope GetScope(ActorContext actor)
    {
        ArgumentNullException.ThrowIfNull(actor);

        return actor.Role switch
        {
            ActorRole.Requester => VisibilityScope.OwnRequests(actor.Id, RequesterActions),

            ActorRole.Staff => VisibilityScope.RequestTypes(
                actor.StaffGroups
                    .SelectMany(group => group.RequestTypes)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase),
                StaffActions),

            ActorRole.Manager => VisibilityScope.All(ManagerActions),

            _ => throw new ArgumentOutOfRangeException(nameof(actor.Role))
        };
    }
}
