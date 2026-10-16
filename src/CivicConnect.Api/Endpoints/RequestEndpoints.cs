using System.Security.Claims;
using CivicConnect.Api.Auth;
using CivicConnect.Api.Infrastructure;
using CivicConnect.Core.Lifecycle;
using CivicConnect.Core.Requests;
using CivicConnect.Data;

namespace CivicConnect.Api.Endpoints;

/// <summary>
/// Service-request endpoints. They only translate HTTP to Core calls: every business and authorization
/// rule stays in RequestService / TransitionService (ADR-04, ADR-05).
/// </summary>
public static class RequestEndpoints
{
    // API contract limits (FR-002). Blank values are already refused by RequestService.
    public const int MaxTitleLength = 120;
    public const int MaxDescriptionLength = 2000;

    public static void MapRequestEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/categories", async (CivicConnectDatabase db) =>
            Results.Ok(await SqliteReferenceData.ListCategoriesAsync(db.ConnectionString)));

        var requests = app.MapGroup("/api/requests");
        requests.MapPost("/", SubmitAsync);
        requests.MapGet("/", ListAsync);
        requests.MapGet("/{id:guid}", GetDetailAsync);
        requests.MapPost("/{id:guid}/transitions", ChangeStatusAsync);
        requests.MapPost("/{id:guid}/comments", AddCommentAsync);
    }

    // FR-002, FR-003: a requester submits a categorised request; it starts as Unassigned.
    private static async Task<IResult> SubmitAsync(SubmitRequestBody body, ClaimsPrincipal user, RequestService service,
        CivicConnectDatabase db, TimeProvider clock)
    {
        if (body.Title?.Trim().Length > MaxTitleLength)
            return ApiResults.Error(400, "InvalidInput", $"Title must be at most {MaxTitleLength} characters.");
        if (body.Description?.Trim().Length > MaxDescriptionLength)
            return ApiResults.Error(400, "InvalidInput", $"Description must be at most {MaxDescriptionLength} characters.");

        // DEF-08: store the catalogue's spelling ("Roads"), not whatever case the client sent ("roads").
        var categories = await SqliteReferenceData.ListCategoriesAsync(db.ConnectionString);
        var category = categories.FirstOrDefault(c => string.Equals(c, body.Category?.Trim(), StringComparison.OrdinalIgnoreCase))
                       ?? body.Category ?? ""; // unknown categories are still refused by RequestService

        var result = await service.SubmitAsync(user.ToActor(), category, body.Title ?? "", body.Description ?? "");
        if (!result.Succeeded)
            return ApiResults.From(result.Failure);

        var created = await service.GetAsync(user.ToActor(), result.RequestId!.Value);
        return Results.Created($"/api/requests/{created!.Id}", RequestSummary.From(created, clock.GetUtcNow()));
    }

    // FR-005, FR-006, FR-014: list only what the caller may see (filtering happens in SQL).
    private static async Task<IResult> ListAsync(ClaimsPrincipal user, RequestService service,
        string? category, string? status, bool? sortByCategory, int? limit, int? offset, bool? overdue, TimeProvider clock)
    {
        if (!TryParseStatus(status, out var parsedStatus))
            return ApiResults.Error(400, "InvalidInput", "Unknown status.");
        var query = new RequestQuery(Category: category, Status: parsedStatus,
            SortByCategory: sortByCategory ?? false, Limit: limit ?? 50, Offset: offset ?? 0, Overdue: overdue);
        if (query.Limit is < 1 or > 100 || query.Offset < 0)
            return ApiResults.Error(400, "InvalidInput", "limit must be 1-100 and offset 0 or more.");

        var found = await service.ListAsync(user.ToActor(), query);
        var now = clock.GetUtcNow();
        return Results.Ok(found.Select(r => RequestSummary.From(r, now)));
    }

    // FR-004, FR-007, FR-008: detail with status history. "Not yours" and "does not exist" both
    // return 404, so other people's request ids cannot be confirmed (NFR-007).
    private static async Task<IResult> GetDetailAsync(Guid id, ClaimsPrincipal user, RequestService service, TimeProvider clock)
    {
        var actor = user.ToActor();
        var request = await service.GetAsync(actor, id);
        if (request is null)
            return ApiResults.Error(404, "RequestNotFound");

        var history = await service.GetHistoryAsync(actor, id);
        var comments = await service.GetCommentsAsync(actor, id);
        return Results.Ok(new RequestDetail(RequestSummary.From(request, clock.GetUtcNow()), request.Description, history, comments));
    }

    // FR-009, FR-011, FR-015: every status change goes through TransitionService.
    private static async Task<IResult> ChangeStatusAsync(Guid id, TransitionBody body, ClaimsPrincipal user,
        TransitionService transitions)
    {
        if (body.To is null)
            return ApiResults.Error(400, "InvalidInput", "'to' must be Assigned, Rejected or Resolved.");

        var result = await transitions.ApplyAsync(new TransitionRequest(
            id, body.To.Value, user.ToActor(), body.AssigneeId, body.Reason, body.ResolutionNote));

        return result.Succeeded
            ? Results.Ok(new { id, status = result.NewStatus })
            : ApiResults.From(result.Failure);
    }

    // FR-010: staff record comments against requests they are allowed to see.
    private static async Task<IResult> AddCommentAsync(Guid id, CommentBody body, ClaimsPrincipal user, RequestService service)
    {
        var result = await service.AddCommentAsync(user.ToActor(), id, body.Content ?? "");
        return result.Succeeded ? Results.NoContent() : ApiResults.From(result.Failure);
    }

    /// <summary>Accepts status names only ("Assigned"), not numbers, and nothing outside the enum.</summary>
    internal static bool TryParseStatus(string? text, out RequestStatus? status)
    {
        status = null;
        if (string.IsNullOrWhiteSpace(text)) return true;
        if (int.TryParse(text, out _) || !Enum.TryParse<RequestStatus>(text, ignoreCase: true, out var parsed))
            return false;
        status = parsed;
        return true;
    }
}
