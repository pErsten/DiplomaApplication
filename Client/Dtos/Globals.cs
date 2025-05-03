using Microsoft.JSInterop;
using System.Text.Json;

namespace Client.Dtos;

public class Globals
{
    public bool IsLoggedIn => User is not null;
    public AuthenticatedUser? User { get; private set; } = null;

    public async Task Logout(IJSRuntime jsRuntime)
    {
        User = null;
        await jsRuntime.InvokeAsync<string>("localStorage.setItem", "userData", null);
    }

    public async Task Login(IJSRuntime jsRuntime, AuthenticatedUser user)
    {
        User = user;
        await jsRuntime.InvokeAsync<string>("localStorage.setItem", "userData", JsonSerializer.Serialize(user));
    }

    public async Task LoadUser(IJSRuntime jsRuntime)
    {
        var userData = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "userData");
        if (!string.IsNullOrEmpty(userData))
        {
            User = JsonSerializer.Deserialize<AuthenticatedUser>(userData);
        }
        
    }
    public async Task UpdateUser(IJSRuntime jsRuntime, AuthenticatedUser user)
    {
        User.AvatarUrl = user.AvatarUrl;
        User.Username = user.Username;
        await jsRuntime.InvokeAsync<string>("localStorage.setItem", "userData", JsonSerializer.Serialize(User));
    }
}

public class AuthenticatedUser
{
    public string? Username { get; set; } = null;
    public string? Token { get; set; } = null;
    public string? AvatarUrl { get; set; } = null;
}