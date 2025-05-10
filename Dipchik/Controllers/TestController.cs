using System.Text.Json;
using Dipchik.Services;
using Shared.Model;

namespace Dipchik.Controllers;

public static class TestController
{
    public static IEndpointRouteBuilder AddTestController(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("Test");

        group.MapGet("/getLocationLocalizations", GetLocationLocalizations);
        group.MapGet("/deleteLocationLocalizations", DeleteLocationLocalizations);

        return builder;
    }

    public static async Task<IResult> GetLocationLocalizations(CacheManager cacheManager, CancellationToken stoppingToken)
    {
        var data = await cacheManager.GetLocationLocalizations(LocalizationCode.UKR, stoppingToken);
        return Results.Ok(JsonSerializer.Serialize(data));
    }

    public static async Task<IResult> DeleteLocationLocalizations(CacheManager cacheManager, CancellationToken stoppingToken)
    {
        await cacheManager.ClearLocationLocalizationsCache();
        return Results.Ok();
    }
}