using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Common.Model.Entities;
using Microsoft.IdentityModel.Tokens;
using Shared.Model;

namespace Dipchik.Services;

public class JwtTokenGenerator
{
    private readonly string jwtKey;
    private readonly SigningCredentials credentials;
    
    public JwtTokenGenerator(IConfiguration configuration)
    {
        jwtKey = configuration.GetValue<string>("Auth:JwtKey");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public string GenerateJwt(Account account)
        => GenerateJwt(account.Login, account.AccountId, account.Roles);

    public string GenerateJwt(string login, string accountId, AccountRolesEnum roles)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, accountId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, accountId),
            new Claim(ClaimTypes.Surname, login),
            new Claim(ClaimTypes.Role, ((int)roles).ToString())
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}