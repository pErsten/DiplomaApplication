using Backend.Services;
using Shared.Model;

namespace Dipchik.Controllers;

public static class GuideController
{
    public static IEndpointRouteBuilder AddGuideController(this IEndpointRouteBuilder builder, params AccountRolesEnum[] roleRequirements)
    {
        var group = builder.MapGroup("Guide");
        foreach (var role in roleRequirements)
        {
            group.RequireAuthorization(role.ToString());
        }

        group.MapGet("GetTours", GetGuideTours);
        group.MapGet("GetTourInstances", GetGuideTourInstances);

        return builder;
    }

    public static async Task<IResult> GetGuideTours(GuideService guideService, HttpContext context, CancellationToken stoppingToken)
    {
        var accountId = context.UserId();
        if (accountId == null)
        {
            return Results.BadRequest(ErrorCodes.ErrorCode_Unauthorized);
        }

        var result = await guideService.GetGuideTours(accountId, stoppingToken);
        if (!result.TryGetValue(out var tours))
        {
            return Results.BadRequest(result.ErrorMessageCode);
        }

        return Results.Ok(tours);
    }

    public static async Task<IResult> GetGuideTourInstances(GuideService guideService, HttpContext context, CancellationToken stoppingToken)
    {
        var accountId = context.UserId();
        if (accountId == null)
        {
            return Results.BadRequest(ErrorCodes.ErrorCode_Unauthorized);
        }

        var result = await guideService.GetGuideTourInstances(accountId, stoppingToken);
        if (!result.TryGetValue(out var tours))
        {
            return Results.BadRequest(result.ErrorMessageCode);
        }

        return Results.Ok(tours);
    }
} 