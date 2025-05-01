using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using Shared.Model.Dtos;
using Shared.Model;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.DbContexts;

namespace Dipchik.Services;

public class CacheManager
{
    private readonly IMemoryCache memCache;
    private readonly IServiceScopeFactory scopeFactory;

    public CacheManager(IMemoryCache memCache, IServiceScopeFactory scopeFactory)
    {
        this.memCache = memCache;
        this.scopeFactory = scopeFactory;
    }

    public void ClearLocationLocalizationsCache()
    {
        foreach (var enumVal in Enum.GetNames<LocalizationCode>())
        {
            memCache.Remove($"{Constants.LOCATION_LOCALIZATION_CACHE_KEY}_{enumVal}");
        }
    }

    public async Task<Dictionary<int, LanguageLocationsDto>> GetLocationLocalizations(LocalizationCode code)
    {
        var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<SqlContext>();
        return await memCache.GetOrCreateAsync($"{Constants.LOCATION_LOCALIZATION_CACHE_KEY}_{code.ToString()}",
            async (cacheEntry) =>
            {
                cacheEntry.AbsoluteExpiration = DateTime.Now.AddMonths(1);
                var data = await dbContext.Languages.Where(x => x.Locale == code).Select(x => x.CitiesAndCountriesJson)
                    .FirstOrDefaultAsync();
                return JsonSerializer.Deserialize<List<LanguageLocationsDto>>(data).ToDictionary(x => x.GeoId);
            });
    }
}