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
    private readonly SignalRService signalRService;

    public EventProceeder(ILoggerFactory loggerFactory, IServiceScopeFactory scopeFactory, ChannelReader<EventDto> eventsChannel, SignalRService signalRService)
    {
        logger = loggerFactory.CreateLogger<EventProceeder>();
        this.scopeFactory = scopeFactory;
        this.eventsChannel = eventsChannel;
        this.signalRService = signalRService;
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
                    case (EventTypeEnum.LocationsLocalizationsUpdated, List<CityLocationDto> dto):
                        await signalRService.SendLocationLocalizationsUpdate(signalRHub.Clients);

                        json = JsonSerializer.Serialize(dto);
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