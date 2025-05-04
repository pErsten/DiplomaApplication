using Dipchik.Services;
using Shared.Model;

namespace Dipchik.Controllers;

public static class LocalizationController
{
    public static IEndpointRouteBuilder AddLocalizationController(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("localization");

        group.MapGet("/LoadLocalization", LoadLocalization);

        return builder;
    }

    public static async Task<IResult> LoadLocalization(string localeCode, HttpContext context, CacheManager cache, CancellationToken stoppingToken)
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
}