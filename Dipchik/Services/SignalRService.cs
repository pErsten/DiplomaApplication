using Microsoft.AspNetCore.SignalR;

namespace Dipchik.Services;

public class SignalRService
{
    private readonly IServiceScopeFactory scopeFactory;

    public SignalRService(IServiceScopeFactory scopeFactory)
    {
        this.scopeFactory = scopeFactory;
    }

    //public async Task SendLocationLocalizationsUpdate(IHubClients clients)
    //{
    //    await clients.All.SendAsync("LocationLocalizationsUpdate");
    //}

    //public async Task GetLocationLocalizationsUpdate(IHubCallerClients clients)
    //{
    //    using var scope = scopeFactory.CreateScope();
    //    var cacheManager = scope.ServiceProvider.GetService<CacheManager>();

    //    cacheManager.ClearLocationLocalizationsCache();
    //}
}