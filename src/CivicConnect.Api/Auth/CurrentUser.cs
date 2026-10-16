using System.Security.Claims;
using CivicConnect.Core.Authorization;

namespace CivicConnect.Api.Auth;

/// <summary>Turns the signed-in cookie identity into the Core's <see cref="ActorContext"/>.</summary>
public static class CurrentUser
{
    /// <remarks>
    /// Only the user id matters here: RequestService and TransitionService reload the actor's role and
    /// staff groups from the database on every call (ADR-05), so a stale or tampered claim cannot widen access.
    /// </remarks>
    public static ActorContext ToActor(this ClaimsPrincipal user) =>
        new(user.FindFirstValue(ClaimTypes.NameIdentifier)!,
            Enum.Parse<ActorRole>(user.FindFirstValue(ClaimTypes.Role)!));
}
