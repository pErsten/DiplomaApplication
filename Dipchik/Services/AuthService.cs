using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.DbContexts;
using Shared.Model.Entities;

namespace Dipchik.Services;

public class AuthService
{
    private ILogger<AuthService> logger;
    private readonly SqlContext dbContext;

    public AuthService(ILoggerFactory loggerFactory, SqlContext dbContext)
    {
        logger = loggerFactory.CreateLogger<AuthService>();
        this.dbContext = dbContext;
    }

    private async Task<Account?> GetExistingUser(string login)
    {
        return await dbContext.Accounts.FirstOrDefaultAsync(x => x.Login == login);
    }

    public async Task<Account> AddNewUser(string login, string passwordHash)
    {
        var account = await GetExistingUser(login);
        if (account is not null)
        {
            logger.LogWarning("Tried to register already existing user: {userGuid}", account.AccountId);
            return account.PasswordHash != passwordHash! ? null! : account;
        }

        account = new Account(passwordHash, login);
        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync();
        return account;
    }

    public async Task<Account?> ValidateUser(string login, string passwordHash)
    {
        var account = await GetExistingUser(login);
        if (account is null)
        {
            logger.LogWarning("User doesn't exist, login: {login}", login);
            return null;
        }

        if (passwordHash != account.PasswordHash)
        {
            logger.LogInformation("Wrong password, login: {login}", login);
            return null;
        }

        return account;
    }

    public static string PasswordHasher(string password)
    {
        using SHA256 sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }
}