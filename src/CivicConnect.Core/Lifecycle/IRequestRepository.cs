using CivicConnect.Core.Authorization;
using CivicConnect.Core.Requests;

namespace CivicConnect.Core.Lifecycle;

/// <summary>
/// Persistence boundary for lifecycle changes. Keeps the lifecycle rules independent of the
/// database (ADR-04) so they can be unit tested without one, and so the data store can change.
/// </summary>
public interface IRequestRepository
{
    Task<ServiceRequest?> GetAsync(Guid requestId, CancellationToken cancellationToken = default);
    Task<ActorContext?> GetActorAsync(string actorId, CancellationToken cancellationToken = default);
    Task<bool> CategoryExistsAsync(string category, CancellationToken cancellationToken = default);
    Task CreateAsync(ServiceRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceRequest>> ListAsync(VisibilityScope scope, RequestQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TransitionRecord>> GetHistoryAsync(Guid requestId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RequestComment>> GetCommentsAsync(Guid requestId, CancellationToken cancellationToken = default);
    Task AddCommentAsync(RequestComment comment, CancellationToken cancellationToken = default);
    /// <summary>Atomically replaces active responsibility and records assignment history, using the expected version.</summary>
    Task<bool> ReassignAsync(ServiceRequest expected, string assigneeId, string actorId, DateTimeOffset at, CancellationToken cancellationToken = default);
    Task<ActivityCounts> GetCountsAsync(VisibilityScope scope, RequestQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists the status change AND its audit entry in ONE atomic operation (NFR-004): both
    /// are saved, or neither is. Must only update when the stored status still equals
    /// <see cref="TransitionRecord.From"/> and its version matches. Returns false on a concurrent change.
    /// Throws on storage failure after rolling back.
    /// </summary>
    Task<bool> SaveTransitionAsync(TransitionRecord record, CancellationToken cancellationToken = default);
}
