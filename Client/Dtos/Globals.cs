using Microsoft.JSInterop;
using System.Text.Json;
using Shared.Model;
using Shared.Model.Dtos;

namespace Client.Dtos;

public class Globals
{
    private readonly IConfiguration configuration;
    public event Action? OnChange;

    public void Notify() => OnChange?.Invoke();

    public bool IsLoggedIn => User is not null || !string.IsNullOrEmpty(Token);
    public AccountInfoDto? User { get; private set; } = null;
    public string? Token { get; set; } = null;
    public Dictionary<string, string> Localizations { get; set; } = null!;

    public Globals(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public async Task LoadLocalization(LocalizationCode oldLocale, LocalizationCode newLocale)
    {
        if (oldLocale == newLocale)
        {
            return;
        }
        var serverUrl = configuration.GetValue<string>("ServerUrl");
        using var cli = new HttpClient();
        var response = await cli.GetAsync($"{serverUrl}/localization/LoadLocalization?localeCode={newLocale.ToString()}");
        var result = await response.Content.ReadAsStringAsync();
        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            return;
        }

        Localizations = JsonSerializer.Deserialize<Dictionary<string, string>>(result);
        Notify();
    }

    public async Task Logout(IJSRuntime jsRuntime)
    {
        User = null;
        Token = null;
        await jsRuntime.InvokeAsync<string>("localStorage.setItem", "userData", null);
        await jsRuntime.InvokeAsync<string>("localStorage.setItem", "userToken", null);
        Notify();
    }

    public async Task Login(IJSRuntime jsRuntime, AccountInfoDto user, string token)
    {
        await LoadLocalization(User?.Locale ?? LocalizationCode.None, user.Locale);
        User = user;
        Token = token;
        await jsRuntime.InvokeAsync<string>("localStorage.setItem", "userData", JsonSerializer.Serialize(user));
        await jsRuntime.InvokeAsync<string>("localStorage.setItem", "userToken", token);
        Notify();
    }

    public async Task LoadUser(IJSRuntime jsRuntime)
    {
        var userData = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "userData");
        Token = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "userToken");
        if (!string.IsNullOrEmpty(userData))
        {
            User = JsonSerializer.Deserialize<AccountInfoDto>(userData);
        }
        await LoadLocalization(LocalizationCode.None, User?.Locale ?? LocalizationCode.ENG);
    }
    public async Task UpdateUser(IJSRuntime jsRuntime, AccountInfoDto user)
    {
        await LoadLocalization(User.Locale, user.Locale);
        User = user;
        await jsRuntime.InvokeAsync<string>("localStorage.setItem", "userData", JsonSerializer.Serialize(User));
        Notify();
    }
}