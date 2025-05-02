using Dipchik.Services;
using Microsoft.AspNetCore.SignalR;

namespace Dipchik.Controllers;

public class SignalRHub : Hub
{
    private readonly SignalRService service;

    public SignalRHub(SignalRService service)
    {
        this.service = service;
    }

    //public async Task LocationLocalizationsUpdate()
    //{
    //    await service.GetLocationLocalizationsUpdate(Clients);
    //}
}