using Common.Model.Entities;

namespace Shared.Model.Dtos;

public class AccountInfoDto
{
    public string Username { get; set; }
    public string AvatarUrl { get; set; }
    public AccountRolesEnum Roles { get; set; }
    public LocalizationCode Locale { get; set; }
    public bool HasRole(AccountRolesEnum role)
        => (Roles & role) > 0;

    public AccountInfoDto() { }

    public AccountInfoDto(Account account)
    {
        AvatarUrl = account.AvatarUrl;
        Username = account.Username;
        Roles = account.Roles;
        Locale = account.Locale;
    }
}