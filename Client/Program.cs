using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Client;
using Client.Dtos;
using MudBlazor.Services;
using Microsoft.JSInterop;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

var globals = new Globals();
builder.Services.AddSingleton<Globals>(x => globals);

builder.Services.AddMudServices();

var host = builder.Build();
var jsRuntime = host.Services.GetRequiredService<IJSRuntime>();
await globals.LoadUser(jsRuntime);

await host.RunAsync();