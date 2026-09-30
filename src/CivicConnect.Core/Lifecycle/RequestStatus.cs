namespace CivicConnect.Core.Lifecycle;

/// <summary>
/// Controlled set of service-request statuses. Fixed by CR-M2-01 (approved status model).
/// Rejected and Resolved are terminal. Resolved also represents "closed".
/// </summary>
public enum RequestStatus
{
    Unassigned = 0, // initial status on submission (FR-002)
    Assigned = 1,   // a staff member owns the request
    Rejected = 2,   // terminal; reason mandatory (FR-007)
    Resolved = 3    // terminal; also the closed state (FR-011)
}
