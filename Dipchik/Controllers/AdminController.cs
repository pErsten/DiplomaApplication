using Dipchik.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.Model;
using Shared.Model.Dtos;

namespace Dipchik.Controllers;

public static class AdminController
{
    public static IEndpointRouteBuilder AddAdminController(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("Admin");

        group.MapGet("/updateLocationsLocalizations", UpdateLocationsLocalizations);
        group.MapGet("/getAllDisplayLocalizations", GetAllDisplayLocalizations);
        group.MapPost("/updateDisplayLocalizations", UpdateDisplayLocalizations);

        return builder;
    }

    public static async Task<IResult> UpdateLocationsLocalizations(LocalizationsService localizationsService, CancellationToken stoppingToken)
    {
        await localizationsService.UpdateLocaleLocalizations(stoppingToken);
        return Results.Ok();
    }

    public static async Task<IResult> UpdateDisplayLocalizations([FromBody] List<LocalizationManagerDto> entries, LocalizationsService localizationsService, CancellationToken stoppingToken)
    {
        var result = new Dictionary<LocalizationCode, Dictionary<string, string>>();

        foreach (var entry in entries)
        {
            foreach (var (langStr, value) in entry.Translations)
            {
                var lang = Enum.Parse<LocalizationCode>(langStr);
                if (!result.TryGetValue(lang, out var langDict))
                {
                    langDict = new Dictionary<string, string>();
                    result[lang] = langDict;
                }

                langDict[entry.Placeholder] = value;
            }
        }

        await localizationsService.UpdateDisplayLocalizations(result, stoppingToken);
        return Results.Ok();
    }

    public static async Task<IResult> GetAllDisplayLocalizations(LocalizationsService localizationsService, CancellationToken stoppingToken)
    {
        var data = await localizationsService.GetAllDisplayLocalizations(stoppingToken);
        var lookup = new Dictionary<string, LocalizationManagerDto>();

        foreach (var (lang, translations) in data)
        {
            foreach (var (key, value) in translations)
            {
                if (!lookup.TryGetValue(key, out var entry))
                {
                    entry = new LocalizationManagerDto { Placeholder = key };
                    lookup[key] = entry;
                }

                entry.Translations[lang.ToString()] = value;
            }
        }
        return Results.Ok(lookup.Values);
    }
}