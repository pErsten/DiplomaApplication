using System.Security.Cryptography;
using System.Text;
using Common.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.DbContexts;
using Shared.Model;

namespace Dipchik.Services;

public class AuthService
{
    private ILogger<AuthService> logger;
    private readonly SqlContext dbContext;
    private readonly HttpContext httpContext;

    public AuthService(ILoggerFactory loggerFactory, SqlContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        logger = loggerFactory.CreateLogger<AuthService>();
        this.dbContext = dbContext;
        this.httpContext = httpContextAccessor.HttpContext;
    }

    private async Task<OperationResult<Account>> GetExistingUser(string login)
    {
        var account = await dbContext.Accounts.FirstOrDefaultAsync(x => x.Login == login);
        if (account is null)
        {
            logger.LogWarning("User doesn't exist, login: {login}", login);
            return new OperationResult<Account>(ErrorCodes.ErrorCode_AccountNotFound);
        }

        return new OperationResult<Account>(account);
    }

    public async Task<OperationResult<Account>> AddNewUser(string login, string passwordHash)
    {
        var result = await GetExistingUser(login);
        if (result.TryGetValue(out var account))
        {
            logger.LogWarning("Tried to register already existing user: {userGuid}", account.AccountId);
            return account.PasswordHash != passwordHash! ? new OperationResult<Account>(ErrorCodes.ErrorCode_TriedToRegisterExistingAccount) : result;
        }

        account = new Account(passwordHash, login);
        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync();
        return new OperationResult<Account>(account);
    }

    public async Task<OperationResult<Account>> ValidateUser(string login, string passwordHash)
    {
        var result = await GetExistingUser(login);
        if (!result.TryGetValue(out var account))
        {
            return result;
        }

        if (passwordHash != account.PasswordHash)
        {
            logger.LogInformation("Wrong password, login: {login}", login);
            return new OperationResult<Account>(ErrorCodes.ErrorCode_AccountWrongPassword);
        }

        return result;
    }

    public async Task<OperationResult<Account>> GetAccount(CancellationToken stoppingToken)
    {
        var accountId = httpContext.UserId();
        if (accountId == null)
        {
            return new OperationResult<Account>(ErrorCodes.ErrorCode_AccountNotFound);
        }

        var account = await dbContext.Accounts.FirstOrDefaultAsync(x => x.AccountId == accountId, stoppingToken);
        if (account is null)
        {
            return new OperationResult<Account>(ErrorCodes.ErrorCode_AccountNotFound);
        }

        return new OperationResult<Account>(account);
    }

    public static string PasswordHasher(string password)
    {
        using SHA256 sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }
}