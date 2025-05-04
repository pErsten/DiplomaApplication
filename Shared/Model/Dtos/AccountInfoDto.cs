namespace Shared.Model.Dtos;

public class AccountInfoDto
{
    public string Username { get; set; }
    public string AvatarUrl { get; set; }
    public AccountRolesEnum Roles { get; set; }
}