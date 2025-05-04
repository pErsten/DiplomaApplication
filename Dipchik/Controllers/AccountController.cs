using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Dipchik.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.DbContexts;
using Npgsql.BackendMessages;
using Shared.Model.Dtos;

namespace Dipchik.Controllers;

public static class AccountController
{
    public static IEndpointRouteBuilder AddAccountController(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("Account");

        group.MapGet("/GetAccountInfo", GetAccountInfo);
        group.MapGet("/DeleteAccount", DeleteAccount);
        group.MapPost("/UpdateUser", UpdateUser);

        return builder;
    }

    public static async Task<IResult> UpdateUser([FromBody]byte[] imageBytes, string Username, HttpContext context, SqlContext dbContext, Cloudinary cloudinary, CancellationToken stoppingToken)
    {
        var accountId = context.UserId();
        var account = await dbContext.Accounts.FirstOrDefaultAsync(x => x.AccountId == accountId, stoppingToken);
        if (account is null)
        {
            return Results.BadRequest("Account not found");
        }

        if (imageBytes.Length > 0)
        {
            Stream stream = new MemoryStream(imageBytes);
            var uploadParams = new ImageUploadParams()
            {
                Overwrite = true,
                DisplayName = $"{account.AccountId}_avatar",
                File = new FileDescription($"{account.AccountId}_avatar", stream)
            };
            var uploadResult = cloudinary.Upload(uploadParams);
            if (uploadResult.Url is null)
            {
                return Results.BadRequest("Failed to upload image to CDN");
            }

            account.AvatarUrl = uploadResult.Url.OriginalString;
        }
        
        account.Username = Username;
        await dbContext.SaveChangesAsync(stoppingToken);
        return Results.Ok(new AccountInfoDto
        {
            AvatarUrl = account.AvatarUrl,
            Username = Username,
            Roles = account.Roles
        });
    }

    public static async Task<IResult> GetAccountInfo(HttpContext context, SqlContext dbContext, CancellationToken stoppingToken)
    {
        var accountId = context.UserId();
        var account = await dbContext.Accounts.FirstOrDefaultAsync(x => x.AccountId == accountId, stoppingToken);
        if (account is null)
        {
            return Results.BadRequest("Account not found");
        }
        
        return Results.Ok(new AccountInfoDto
        {
            AvatarUrl = account.AvatarUrl,
            Username = account.Username,
            Roles = account.Roles
        });
    }

    public static async Task<IResult> DeleteAccount(HttpContext context, SqlContext dbContext, CancellationToken stoppingToken)
    {
        var accountId = context.UserId();
        var account = await dbContext.Accounts.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.AccountId == accountId, stoppingToken);
        if (account is null)
        {
            return Results.BadRequest("Account not found");
        }
        if (account.IsDeleted)
        {
            return Results.BadRequest("Account already deleted");
        }

        account.IsDeleted = true;
        account.UtcDeleted = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(stoppingToken);

        return Results.Ok();
    }
}