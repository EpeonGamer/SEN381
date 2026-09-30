namespace CivicConnect.Core.Lifecycle;

/// <summary>
/// Persistence boundary for lifecycle changes. Keeps the lifecycle rules independent of the
/// database (ADR-04) so they can be unit tested without one, and so the data store can change.
/// </summary>
public interface IRequestRepository
{
    /// <summary>Returns the current status, or null if the request does not exist.</summary>
    Task<RequestStatus?> GetStatusAsync(Guid requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists the status change AND its audit entry in ONE atomic operation (NFR-004): both
    /// are saved, or neither is. Must only update when the stored status still equals
    /// <see cref="TransitionRecord.From"/>. Returns false if it no longer does (concurrent change).
    /// Throws on storage failure after rolling back.
    /// </summary>
    Task<bool> SaveTransitionAsync(TransitionRecord record, CancellationToken cancellationToken = default);
}
