using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;

namespace Void.Bot.Blazor.WASM.Services;

public class AuthService
{
    private readonly IJSRuntime _js;
    private readonly HttpClient _httpClient;
    private const string UsernameKey = "auth_username";
    private const string AccessTokenKey = "auth_access_token";
    private const string RefreshTokenKey = "auth_refresh_token";

    public bool IsLoggedIn { get; private set; }
    public string? Username { get; private set; }
    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }

    public event Action? OnChange;

    public AuthService(IJSRuntime js, HttpClient httpClient)
    {
        _js = js;
        _httpClient = httpClient;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // 先检查 sessionStorage（临时会话），再检查 localStorage（持久化会话）
            var username = await _js.InvokeAsync<string?>("sessionStorage.getItem", UsernameKey);
            var accessToken = await _js.InvokeAsync<string?>("sessionStorage.getItem", AccessTokenKey);
            var refreshToken = await _js.InvokeAsync<string?>("sessionStorage.getItem", RefreshTokenKey);

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(accessToken))
            {
                username = await _js.InvokeAsync<string?>("localStorage.getItem", UsernameKey);
                accessToken = await _js.InvokeAsync<string?>("localStorage.getItem", AccessTokenKey);
                refreshToken = await _js.InvokeAsync<string?>("localStorage.getItem", RefreshTokenKey);
            }

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(accessToken))
            {
                IsLoggedIn = true;
                Username = username;
                AccessToken = accessToken;
                RefreshToken = refreshToken;
                OnChange?.Invoke();
            }
        }
        catch
        {
            // localStorage/sessionStorage 不可用时忽略
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> LoginAsync(
        string name,
        string suffix,
        string password,
        bool rememberMe)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/Identity/login?Name={name}&Suffix={suffix}&Password={password}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadFromJsonAsync<ApiResponse>();
                return (false, errorContent?.Message ?? "登录失败");
            }

            var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
            if (content?.Code != 200 || content.Data == null)
            {
                return (false, content?.Message ?? "登录失败");
            }

            IsLoggedIn = true;
            Username = $"{name}#{suffix}";
            AccessToken = content.Data.AccessToken;
            RefreshToken = content.Data.RefreshToken;

            await SaveToStorageAsync(rememberMe);
            OnChange?.Invoke();
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"网络错误: {ex.Message}");
        }
    }

    public string? GetAccessToken() => AccessToken;

    public async void Logout()
    {
        IsLoggedIn = false;
        Username = null;
        AccessToken = null;
        RefreshToken = null;
        await RemoveFromStorageAsync();
        OnChange?.Invoke();
    }

    private async Task SaveToStorageAsync(bool rememberMe)
    {
        try
        {
            var storage = rememberMe ? "localStorage" : "sessionStorage";
            await _js.InvokeVoidAsync($"{storage}.setItem", UsernameKey, Username);
            await _js.InvokeVoidAsync($"{storage}.setItem", AccessTokenKey, AccessToken);
            await _js.InvokeVoidAsync($"{storage}.setItem", RefreshTokenKey, RefreshToken);

            if (rememberMe)
            {
                await _js.InvokeVoidAsync("localStorage.setItem", $"{RefreshTokenKey}_expiry", DateTimeOffset.UtcNow.AddDays(30).ToString());
            }
        }
        catch
        {
            // localStorage/sessionStorage 不可用时忽略
        }
    }

    private async Task RemoveFromStorageAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", UsernameKey);
            await _js.InvokeVoidAsync("sessionStorage.removeItem", UsernameKey);
            await _js.InvokeVoidAsync("localStorage.removeItem", AccessTokenKey);
            await _js.InvokeVoidAsync("sessionStorage.removeItem", AccessTokenKey);
            await _js.InvokeVoidAsync("localStorage.removeItem", RefreshTokenKey);
            await _js.InvokeVoidAsync("sessionStorage.removeItem", RefreshTokenKey);
        }
        catch
        {
            // localStorage 不可用时忽略
        }
    }

    private class ApiResponse
    {
        public int Code { get; set; }
        public string? Message { get; set; }
        public TokenData? Data { get; set; }
    }

    private class TokenData
    {
        public string AccessToken { get; set; } = "";
        public string RefreshToken { get; set; } = "";
    }
}
