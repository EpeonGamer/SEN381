namespace CivicConnect.Core.Authorization;

/// <summary>Minimum request information needed to evaluate visibility.</summary>
public sealed record RequestAccess(
    Guid RequestId,
    string RequesterId,
    string RequestType);
