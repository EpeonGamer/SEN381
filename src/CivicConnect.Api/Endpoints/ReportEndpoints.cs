using System.Security.Claims;
using CivicConnect.Api.Auth;
using CivicConnect.Core.Requests;

namespace CivicConnect.Api.Endpoints;

/// <summary>Management overview (FR-012, FR-013, FR-014).</summary>
public static class ReportEndpoints
{
    public static void MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        // The role check here gives a clean 403; RequestService checks the role again (defence in depth).
        app.MapGet("/api/reports/counts", GetCountsAsync)
            .RequireAuthorization(policy => policy.RequireRole("Manager"));
    }

    // Open = Unassigned + Assigned. "Overdue" is not defined in the approved model yet (DEF-07).
    private static async Task<IResult> GetCountsAsync(ClaimsPrincipal user, RequestService service, string? category, string? status)
    {
        if (!RequestEndpoints.TryParseStatus(status, out var parsedStatus))
            return ApiResults.Error(400, "InvalidInput", "Unknown status.");

        var counts = await service.GetCountsAsync(user.ToActor(), new RequestQuery(Category: category, Status: parsedStatus));
        return Results.Ok(new ActivityCountsView(counts.Open, counts.Rejected, counts.Resolved));
    }
}
