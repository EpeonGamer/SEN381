using CivicConnect.Core.Lifecycle;

namespace CivicConnect.Core.Requests;

/// <summary>FR-013: overdue is derived, never a lifecycle status. See CR-M3-02.</summary>
public static class OverduePolicy
{
    public static TimeSpan ResolutionWindow => TimeSpan.FromDays(3);

    public static DateTimeOffset DueAt(ServiceRequest request) => request.SubmittedAt + ResolutionWindow;

    public static DateTimeOffset Cutoff(DateTimeOffset now) => now - ResolutionWindow;

    public static bool IsOverdue(ServiceRequest request, DateTimeOffset now) =>
        request.Status is RequestStatus.Unassigned or RequestStatus.Assigned && now > DueAt(request);
}
