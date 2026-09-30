namespace CivicConnect.Core.Authorization;

/// <summary>Central authorization/visibility policy used by application services.</summary>
public interface IVisibilityPolicy
{
    VisibilityScope GetScope(ActorContext actor);
}
