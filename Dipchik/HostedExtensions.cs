using System.Security.Claims;
using System.Text;
using System.Threading.Channels;
using CloudinaryDotNet;
using Dipchik.BackgroundWorkers;
using Dipchik.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.DbContexts;
using Microsoft.IdentityModel.Tokens;
using Shared.Model;
using Shared.Model.Dtos;
using StackExchange.Redis;

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
            var redisConnectionStr = builder.Configuration.GetValue<string>("Databases:RedisConnection"); ;
            var cloudinaryConnectionStr = builder.Configuration.GetValue<string>("Databases:CloudinaryConnection"); ;
            
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(redisConnectionStr));
            services.AddSignalR().AddStackExchangeRedis(redisConnectionStr);
            services.AddDbContext<SqlContext>(options => options.UseNpgsql(sqlConnectionStr));
            services.AddScoped<AuthService>();
            services.AddScoped<LocalizationsService>();
            services.AddSingleton<Cloudinary>(x =>
            {
                var elem = new Cloudinary(cloudinaryConnectionStr);
                elem.Api.Secure = true;
                return elem;
            });
            services.AddSingleton<JwtTokenGenerator>();
            services.AddSingleton<SignalRService>();
            services.AddSingleton<CacheManager>();
            services.AddHttpContextAccessor();

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

            services.AddAuthorization(options =>
            {
                foreach (AccountRolesEnum elem in Enum.GetValues<AccountRolesEnum>())
                {
                    if (elem == AccountRolesEnum.None)
                    {
                        continue;
                    }
                    options.AddPolicy(elem.ToString(), policy => policy.RequireAssertion(context =>
                    {
                        var rolesClaim = context.User.FindFirst(ClaimTypes.Role)?.Value;
                        if (string.IsNullOrEmpty(rolesClaim)) 
                            return false;
                        if (!int.TryParse(rolesClaim, out var roleFlags))
                            return false;

                        return (roleFlags & (int)elem) > 0;
                    }));
                }
            });
            services.AddSignalR();
            var webClientUrl = builder.Configuration.GetValue<string>("WebClientUrl");
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
