using Dipchik.Services;
using Shared.Model;

namespace Dipchik.Controllers;

public static class LocalizationController
{
    public static IEndpointRouteBuilder AddLocalizationController(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("localization");

        group.MapGet("/LoadDisplayLocalization", LoadDisplayLocalization);
        group.MapGet("/LoadLocationLocalization", LoadLocationLocalization);

        return builder;
    }

    public static async Task<IResult> LoadDisplayLocalization(string localeCode, CacheManager cache, CancellationToken stoppingToken)
    {
        if (!Enum.TryParse<LocalizationCode>(localeCode, out var localization))
        {
            return Results.BadRequest("Bad localization code provided");
        }
        if (localization == LocalizationCode.None)
        {
            return Results.BadRequest("Localization can't be None");
        }

        var res = await cache.GetDisplayLocalizations(localization, stoppingToken);
        return Results.Ok(res);
    }

    public static async Task<IResult> LoadLocationLocalization(string localeCode, CacheManager cache, CancellationToken stoppingToken)
    {
        if (!Enum.TryParse<LocalizationCode>(localeCode, out var localization))
        {
            return Results.BadRequest("Bad localization code provided");
        }
        if (localization == LocalizationCode.None)
        {
            return Results.BadRequest("Localization can't be None");
        }

        var res = await cache.GetLocationLocalizations(localization, stoppingToken);
        return Results.Ok(res.Values.ToList());
    }
}