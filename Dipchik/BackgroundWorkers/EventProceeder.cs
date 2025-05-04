using Common.Model.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting.DbContexts;
using Shared.Model.Dtos;
using Shared.Model;
using System.Text.Json;
using System.Threading.Channels;
using Dipchik.Controllers;
using Dipchik.Services;

namespace Dipchik.BackgroundWorkers;

public class EventProceeder : BackgroundService
{
    private readonly ILogger<EventProceeder> logger;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ChannelReader<EventDto> eventsChannel;
    private readonly CacheManager cacheManager;

    public EventProceeder(ILoggerFactory loggerFactory, IServiceScopeFactory scopeFactory, ChannelReader<EventDto> eventsChannel, CacheManager cacheManager)
    {
        logger = loggerFactory.CreateLogger<EventProceeder>();
        this.scopeFactory = scopeFactory;
        this.eventsChannel = eventsChannel;
        this.cacheManager = cacheManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var newEvent in eventsChannel.ReadAllAsync(stoppingToken))
        {
            try
            {
                var scope = scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetService<SqlContext>();
                var signalRHub = scope.ServiceProvider.GetService<IHubContext<SignalRHub>>();
                string json = string.Empty;
                switch (newEvent.EventType, newEvent.EventBody)
                {
                    case (EventTypeEnum.LocationsLocalizationsUpdated, List<CityLocationDto> data):
                        await cacheManager.ClearLocationLocalizationsCache();

                        json = JsonSerializer.Serialize(data);
                        await dbContext.Events.AddAsync(new AppEvent(newEvent, json), stoppingToken);
                        await dbContext.SaveChangesAsync(stoppingToken);
                        break;
                    case (EventTypeEnum.DisplayLocalizationsUpdated, Dictionary<LocalizationCode, Dictionary<string, string>> data):
                        await cacheManager.ClearDisplayLocalizationsCache();

                        json = JsonSerializer.Serialize(data);
                        await dbContext.Events.AddAsync(new AppEvent(newEvent, json), stoppingToken);
                        await dbContext.SaveChangesAsync(stoppingToken);
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError("EventProceeder ex: {exception}", ex);
            }
        }
    }
}