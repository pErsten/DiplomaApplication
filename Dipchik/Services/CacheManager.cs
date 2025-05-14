using Shared.Model.Dtos;
using Shared.Model;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.DbContexts;
using StackExchange.Redis;

namespace Dipchik.Services;

public class CacheManager
{
    private readonly IDatabase redisDB;
    private readonly IConfiguration configuration;
    private readonly IConnectionMultiplexer connectionMultiplexer;
    private readonly IServiceScopeFactory scopeFactory;
    private ILogger<CacheManager> logger;
    private readonly int localizationsDbInt;

    public CacheManager(ILoggerFactory loggerFactory, IConfiguration configuration, IConnectionMultiplexer connectionMultiplexer, IServiceScopeFactory scopeFactory)
    {
        logger = loggerFactory.CreateLogger<CacheManager>();
        localizationsDbInt = configuration.GetValue<int>("Redis:LocalizationsDbInt");

        redisDB = connectionMultiplexer.GetDatabase(localizationsDbInt);
        this.configuration = configuration;
        this.connectionMultiplexer = connectionMultiplexer;
        this.scopeFactory = scopeFactory;
    }

    public async Task ClearLocationLocalizationsCache()
    {

        var endpoints = connectionMultiplexer.GetEndPoints();
        var server = connectionMultiplexer.GetServer(endpoints[0]);
        
        var keys = server.Keys(database: localizationsDbInt, pattern: $"{Constants.LOCATION_LOCALIZATION_CACHE_KEY}_*").ToArray();

        foreach (var key in keys)
        {
            await redisDB.KeyDeleteAsync(key);
            logger.LogInformation("Clearing Location Localization for {lang} language", key.ToString());
        }
    }
    public async Task ClearDisplayLocalizationsCache()
    {

        var endpoints = connectionMultiplexer.GetEndPoints();
        var server = connectionMultiplexer.GetServer(endpoints[0]);
        
        var keys = server.Keys(database: localizationsDbInt, pattern: $"{Constants.DISPLAY_LOCALIZATION_CACHE_KEY}_*").ToArray();

        foreach (var key in keys)
        {
            await redisDB.KeyDeleteAsync(key);
            logger.LogInformation("Clearing Display Localization for {lang} language", key.ToString());
        }
    }

    public async Task<Dictionary<int, LanguageLocationsDto>> GetLocationLocalizations(LocalizationCode code, CancellationToken stoppingToken)
    {
        var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<SqlContext>();

        var transactionRowValue = await redisDB.HashGetAllAsync($"{Constants.LOCATION_LOCALIZATION_CACHE_KEY}_{code.ToString()}");
        if (transactionRowValue.Any())
        {
            return transactionRowValue.ToDictionary(x => int.Parse(x.Name), x => JsonSerializer.Deserialize<LanguageLocationsDto>(x.Value));
        }

        var data = await dbContext.Languages.Where(x => x.Locale == code).Select(x => x.CitiesAndCountriesJson)
            .FirstOrDefaultAsync(stoppingToken);
        var list = JsonSerializer.Deserialize<List<LanguageLocationsDto>>(data);
        await redisDB.HashSetAsync($"{Constants.LOCATION_LOCALIZATION_CACHE_KEY}_{code.ToString()}",
            list.Select(x => new HashEntry(x.GeoId.ToString(), JsonSerializer.Serialize(x))).ToArray());
        logger.LogInformation("Setting Location Localization for {lang} language", code.ToString());

        return list.ToDictionary(x => x.GeoId);
    }

    public async Task<Dictionary<string, string>> GetDisplayLocalizations(LocalizationCode code, CancellationToken stoppingToken)
    {
        var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<SqlContext>();

        var transactionRowValue = await redisDB.HashGetAllAsync($"{Constants.DISPLAY_LOCALIZATION_CACHE_KEY}_{code.ToString()}");
        if (transactionRowValue.Any())
        {
            return transactionRowValue.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());
        }

        var data = await dbContext.Languages.Where(x => x.Locale == code).Select(x => x.DisplayLocalizationsJson).FirstOrDefaultAsync(stoppingToken);
        var list = JsonSerializer.Deserialize<Dictionary<string, string>>(data);
        await redisDB.HashSetAsync($"{Constants.DISPLAY_LOCALIZATION_CACHE_KEY}_{code.ToString()}",
            list.Select(x => new HashEntry(x.Key, x.Value)).ToArray());
        logger.LogInformation("Setting Display Localization for {lang} language", code.ToString());

        return list.ToDictionary(x => x.Key, x => x.Value);
    }
}