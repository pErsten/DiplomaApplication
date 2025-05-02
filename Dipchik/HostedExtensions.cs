using System.Text;
using System.Threading.Channels;
using Dipchik.BackgroundWorkers;
using Dipchik.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.DbContexts;
using Microsoft.IdentityModel.Tokens;
using Shared.Model;
using Shared.Model.Dtos;

namespace Dipchik
{
    public static class HostedExtensions
    {
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
        {
            var services = builder.Services;

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            var eventsChannel = Channel.CreateUnbounded<EventDto>(new UnboundedChannelOptions
            {
                SingleReader = true
            });
            services.AddSingleton(eventsChannel);
            services.AddSingleton(eventsChannel.Writer);
            services.AddSingleton(eventsChannel.Reader);

            var sqlConnectionStr = builder.Configuration.GetValue<string>("Databases:SqlConnection");
            services.AddDbContext<SqlContext>(options => options.UseNpgsql(sqlConnectionStr));

            services.AddSignalR().AddStackExchangeRedis("redis:6379");

            services.AddScoped<AuthService>();
            services.AddScoped<LocationsParser>();
            services.AddSingleton<JwtTokenGenerator>();
            services.AddSingleton<SignalRService>();
            services.AddSingleton<CacheManager>();

            services.AddHostedService<EventProceeder>();

            var jwtKey = builder.Configuration.GetValue<string>("Auth:JwtKey");
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateLifetime = true,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            if (!string.IsNullOrEmpty(accessToken) &&
                                context.HttpContext.Request.Path.StartsWithSegments("/messages"))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization();
            services.AddSignalR();
            //TODO: add in configs
            var webClientUrl = "";//builder.Configuration.GetValue<string>("WebClientUrl");
            services.AddCors(options =>
            {
                options.AddPolicy("WebClient",
                    policy => policy.WithOrigins(webClientUrl)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
            });

            return builder.Build();
        }
    }
}
