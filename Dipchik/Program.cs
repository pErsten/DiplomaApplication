using Serilog.Events;
using Serilog;
using Dipchik;
using Dipchik.Controllers;
using Shared.Model;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Information)
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();
builder.AddRedisClient("cache");

var app = builder.ConfigureServices();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("WebClient");
app.MapHub<SignalRHub>("/messages");
app.UseHttpsRedirection();
app.UseAuthorization();

// Non auth controllers
var anonEPs = app.MapGroup("/").AllowAnonymous().WithOpenApi();
anonEPs.AddAuthController();
anonEPs.AddLocalizationController();
anonEPs.AddToursController();
if (app.Environment.IsDevelopment())
{
    anonEPs.AddTestController();
}

// Auth controllers
var authEPs = app.MapGroup("/").RequireAuthorization().WithOpenApi();
authEPs.AddAccountController(AccountRolesEnum.Client);
authEPs.AddBookingController(AccountRolesEnum.Client);
authEPs.AddAdminController(AccountRolesEnum.Client, AccountRolesEnum.Modify);
authEPs.AddGuideController(AccountRolesEnum.Client, AccountRolesEnum.Guide);

app.Run();
