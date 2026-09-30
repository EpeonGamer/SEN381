using CivicConnect.Core.Authorization;
using Xunit;

namespace CivicConnect.Core.Tests;

public class AuthorizationTests
{
    private static readonly Guid RequestId = Guid.NewGuid();

    [Fact]
    public void Requester_can_view_only_their_own_requests()
    {
        var actor = new ActorContext("requester-1", ActorRole.Requester);
        var scope = new RoleVisibilityPolicy().GetScope(actor);

        Assert.True(scope.CanView(new RequestAccess(RequestId, "requester-1", "Roads")));
        Assert.False(scope.CanView(new RequestAccess(RequestId, "requester-2", "Roads")));
    }

    [Fact]
    public void Staff_visibility_is_the_union_of_all_group_request_types()
    {
        var actor = new ActorContext(
            "staff-1",
            ActorRole.Staff,
            new[]
            {
                new StaffGroup("Roads", new HashSet<string> { "Road", "Streetlight" }),
                new StaffGroup("Waste", new HashSet<string> { "Waste" })
            });

        var scope = new RoleVisibilityPolicy().GetScope(actor);

        Assert.True(scope.CanView(new RequestAccess(RequestId, "requester-1", "Road")));
        Assert.True(scope.CanView(new RequestAccess(RequestId, "requester-1", "Waste")));
        Assert.False(scope.CanView(new RequestAccess(RequestId, "requester-1", "Security")));
    }

    [Fact]
    public void Manager_can_view_requests_but_cannot_change_status_through_this_policy()
    {
        var actor = new ActorContext("manager-1", ActorRole.Manager);
        var scope = new RoleVisibilityPolicy().GetScope(actor);

        Assert.True(scope.CanView(new RequestAccess(RequestId, "requester-1", "Anything")));
        Assert.False(scope.Allows(AuthorizationAction.ChangeStatus));
    }

    [Fact]
    public void Staff_can_change_status_when_authorized_by_the_policy()
    {
        var actor = new ActorContext(
            "staff-1",
            ActorRole.Staff,
            new[] { new StaffGroup("Roads", new HashSet<string> { "Road" }) });

        var scope = new RoleVisibilityPolicy().GetScope(actor);

        Assert.True(scope.Allows(AuthorizationAction.ChangeStatus));
    }
}
