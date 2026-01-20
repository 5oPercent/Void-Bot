using Microsoft.JSInterop;

namespace Void.Bot.Blazor.WASM.Services;

public class ThemeService
{
    private readonly IJSRuntime _js;
    private const string ThemeStorageKey = "app_theme";

    public bool IsDarkTheme { get; private set; } = true;
    public event Action? OnThemeChanged;

    public ThemeService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var savedTheme = await _js.InvokeAsync<string?>("localStorage.getItem", ThemeStorageKey);
            IsDarkTheme = savedTheme != "light";
            await ApplyThemeAsync();
        }
        catch
        {
            // localStorage 不可用时使用默认暗色主题
            await ApplyThemeAsync();
        }
    }

    public async Task ToggleThemeAsync()
    {
        IsDarkTheme = !IsDarkTheme;
        await ApplyThemeAsync();
        await SaveThemeAsync();
        OnThemeChanged?.Invoke();
    }

    private async Task ApplyThemeAsync()
    {
        try
        {
            if (IsDarkTheme)
            {
                await _js.InvokeVoidAsync("document.body.classList.remove", "light-theme");
                await _js.InvokeVoidAsync("document.body.classList.add", "dark-theme");
            }
            else
            {
                await _js.InvokeVoidAsync("document.body.classList.remove", "dark-theme");
                await _js.InvokeVoidAsync("document.body.classList.add", "light-theme");
            }
        }
        catch
        {
            // 忽略 JS 错误
        }
    }

    private async Task SaveThemeAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", ThemeStorageKey, IsDarkTheme ? "dark" : "light");
        }
        catch
        {
            // 忽略 localStorage 错误
        }
    }
}
