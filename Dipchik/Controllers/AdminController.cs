using Dipchik.Services;

namespace Dipchik.Controllers;

public static class AdminController
{
    public static IEndpointRouteBuilder UserAdminController(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("Admin");

        group.MapGet("/updateLocationsLocalizations", UpdateLocationsLocalizations);

        return builder;
    }

    public static async Task<IResult> UpdateLocationsLocalizations(LocationsParser parser, CancellationToken stoppingToken)
    {
        await parser.UpdateLocalizations(stoppingToken);
        return Results.Ok();
    }
}