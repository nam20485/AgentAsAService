using Microsoft.JSInterop;

namespace OrchestratorWebApp.Services;

/// <summary>
/// Service for managing application themes
/// </summary>
public interface IThemeService
{
    Task<string> GetCurrentThemeAsync();
    Task SetThemeAsync(string theme);
    Task<bool> IsDarkModeAsync();
    Task ToggleThemeAsync();
    event Action<string>? ThemeChanged;
}

/// <summary>
/// Implementation of theme service using localStorage
/// </summary>
public class ThemeService : IThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private string _currentTheme = "light";

    public event Action<string>? ThemeChanged;

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<string> GetCurrentThemeAsync()
    {
        try
        {
            var theme = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "theme");
            _currentTheme = string.IsNullOrEmpty(theme) ? "light" : theme;
            return _currentTheme;
        }
        catch
        {
            return "light";
        }
    }

    public async Task SetThemeAsync(string theme)
    {
        try
        {
            _currentTheme = theme;
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "theme", theme);
            await _jsRuntime.InvokeVoidAsync("document.documentElement.setAttribute", "data-theme", theme);
            ThemeChanged?.Invoke(theme);
        }
        catch
        {
            // Handle JS interop errors gracefully
        }
    }

    public async Task<bool> IsDarkModeAsync()
    {
        var theme = await GetCurrentThemeAsync();
        return theme == "dark";
    }

    public async Task ToggleThemeAsync()
    {
        var currentTheme = await GetCurrentThemeAsync();
        var newTheme = currentTheme == "light" ? "dark" : "light";
        await SetThemeAsync(newTheme);
    }
}

/// <summary>
/// JavaScript functions for theme management
/// </summary>
public static class ThemeJavaScriptFunctions
{
    public const string InitializeTheme = @"
        window.initializeTheme = () => {
            const savedTheme = localStorage.getItem('theme') || 'light';
            document.documentElement.setAttribute('data-theme', savedTheme);
            return savedTheme;
        };
    ";

    public const string ToggleTheme = @"
        window.toggleTheme = () => {
            const current = document.documentElement.getAttribute('data-theme') || 'light';
            const newTheme = current === 'light' ? 'dark' : 'light';
            document.documentElement.setAttribute('data-theme', newTheme);
            localStorage.setItem('theme', newTheme);
            return newTheme;
        };
    ";
}
