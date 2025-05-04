using Shared.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Model.Entities;

public class Account
{
    [Key]
    public int Id { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? UtcDeleted { get; set; }
    public string AccountId { get; set; }
    public string Login { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public DateTime UtcCreated { get; set; }
    public AccountRolesEnum Roles { get; set; }
    public LocalizationCode Locale { get; set; }
    public string? AvatarUrl { get; set; }


    public Account() { }
    public Account(string passwordHash, string login)
    {
        IsDeleted = false;
        PasswordHash = passwordHash;
        Login = login;
        Username = login;
        AccountId = Guid.NewGuid().ToString();
        Locale = LocalizationCode.ENG;
        UtcCreated = DateTime.UtcNow;
        Roles = AccountRolesEnum.Client;
    }
}