using System.Text;
using Dipchik.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.DbContexts;
using Microsoft.IdentityModel.Tokens;

namespace Dipchik
{
    public static class HostedExtensions
    {
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
        {
            var services = builder.Services;

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            var sqlConnectionStr = builder.Configuration.GetValue<string>("Databases:SqlConnection");
            services.AddDbContext<SqlContext>(options => options.UseNpgsql(sqlConnectionStr));

            services.AddScoped<AuthService>();
            services.AddScoped<LocationsParser>();
            services.AddSingleton<JwtTokenGenerator>();

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

            return builder.Build();
        }
    }
}
