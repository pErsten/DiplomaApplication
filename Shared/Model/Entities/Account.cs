﻿using Shared.Model;
using System.ComponentModel.DataAnnotations;

namespace Common.Model.Entities;

public class Account
{
    [Key]
    public int Id { get; set; }
    public string AccountId { get; set; }
    public string Login { get; set; }
    public string PasswordHash { get; set; }
    public DateTime UtcCreated { get; set; }
    public AccountRolesEnum Roles { get; set; }


    public Account() { }
    public Account(string passwordHash, string login)
    {
        PasswordHash = passwordHash;
        Login = login;
        AccountId = Guid.NewGuid().ToString();
        UtcCreated = DateTime.UtcNow;
    }
}