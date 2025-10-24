using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.App.Shared.Services;

public interface IThemeService
{
    event Action<string>? OnThemeChanged;
    Task<string> GetCurrentThemeAsync();
    Task SetThemeAsync(string themeId);
}

public class ThemeService : IThemeService
{
    private const string StorageKey = "flowforge_theme";
    private string _currentTheme = "tech-blue";

    public event Action<string>? OnThemeChanged;

    public Task<string> GetCurrentThemeAsync()
    {
        // 从 localStorage 读取
        return Task.FromResult(_currentTheme);
    }

    public async Task SetThemeAsync(string themeId)
    {
        _currentTheme = themeId;

        // 保存到 localStorage
        // await JS.InvokeVoidAsync("localStorage.setItem", StorageKey, themeId);

        // 更新 DOM
        //await JS.InvokeVoidAsync("document.documentElement.setAttribute", "data-theme", themeId);

        // 触发事件
        OnThemeChanged?.Invoke(themeId);
    }
}