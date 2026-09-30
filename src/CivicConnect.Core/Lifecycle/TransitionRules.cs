namespace CivicConnect.Core.Lifecycle;

/// <summary>Information that must accompany a transition before it is accepted.</summary>
[Flags]
public enum TransitionRequirement
{
    None = 0,
    Assignee = 1,       // FR-009
    Reason = 2,         // FR-007
    ResolutionNote = 4  // FR-011, FR-010
}

/// <summary>
/// The single authoritative definition of allowed request transitions (ADR-04, CR-M2-01 section 3).
/// Any (from, to) pair not listed here is refused. To change the lifecycle, change this table
/// through a controlled change request, then update the state diagram, tests and RTM.
/// </summary>
public static class TransitionRules
{
    private static readonly IReadOnlyDictionary<(RequestStatus From, RequestStatus To), TransitionRequirement> Allowed =
        new Dictionary<(RequestStatus, RequestStatus), TransitionRequirement>
        {
            [(RequestStatus.Unassigned, RequestStatus.Assigned)] = TransitionRequirement.Assignee,
            [(RequestStatus.Unassigned, RequestStatus.Rejected)] = TransitionRequirement.Reason,
            [(RequestStatus.Assigned, RequestStatus.Resolved)] = TransitionRequirement.ResolutionNote
        };

    /// <summary>Returns true when the transition is allowed and outputs what it requires.</summary>
    public static bool TryGetRequirement(RequestStatus from, RequestStatus to, out TransitionRequirement requirement) =>
        Allowed.TryGetValue((from, to), out requirement);

    /// <summary>All allowed transitions, for tests and documentation checks.</summary>
    public static IEnumerable<(RequestStatus From, RequestStatus To)> AllowedTransitions => Allowed.Keys;
}
