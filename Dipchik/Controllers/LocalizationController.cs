using Dipchik.Services;
using Shared.Model;

namespace Dipchik.Controllers;

public static class LocalizationController
{
    public static IEndpointRouteBuilder AddLocalizationController(this IEndpointRouteBuilder builder, params AccountRolesEnum[] roleRequirements)
    {
        var group = builder.MapGroup("localization");
        foreach (var role in roleRequirements)
        {
            group.RequireAuthorization(role.ToString());
        }

        group.MapGet("/LoadDisplayLocalization", LoadDisplayLocalization);
        group.MapGet("/LoadLocationLocalization", LoadLocationLocalization);

        return builder;
    }

    public static async Task<IResult> LoadDisplayLocalization(string localeCode, CacheManager cache, CancellationToken stoppingToken)
    {
        if (!Enum.TryParse<LocalizationCode>(localeCode, out var localization))
        {
            return Results.BadRequest(ErrorCodes.ErrorCode_LocaleNotFound);
        }
        if (localization == LocalizationCode.None)
        {
            return Results.BadRequest(ErrorCodes.ErrorCode_LocaleNotSupported);
        }

        var res = await cache.GetDisplayLocalizations(localization, stoppingToken);
        return Results.Ok(res);
    }

    public static async Task<IResult> LoadLocationLocalization(string localeCode, CacheManager cache, CancellationToken stoppingToken)
    {
        if (!Enum.TryParse<LocalizationCode>(localeCode, out var localization))
        {
            return Results.BadRequest(ErrorCodes.ErrorCode_LocaleNotFound);
        }
        if (localization == LocalizationCode.None)
        {
            return Results.BadRequest(ErrorCodes.ErrorCode_LocaleNotSupported);
        }

        var res = await cache.GetLocationLocalizations(localization, stoppingToken);
        return Results.Ok(res);
    }
}