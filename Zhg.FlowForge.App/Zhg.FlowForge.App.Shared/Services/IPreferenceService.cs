using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using Zhg.FlowForge.App.Shared.Interfaces;

namespace Zhg.FlowForge.App.Shared.Services;



public class BlazorPreferenceService : IPreferenceService
{
    private readonly IJSRuntime _jsRuntime;
    private const string StoragePrefix = "prefs_";

    public BlazorPreferenceService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<string> GetAsync(string key, string defaultValue)
    {
        try
        {
            var storageKey = $"{StoragePrefix}{key}";
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", storageKey)
                ?? defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    public async Task SetAsync(string key, string value)
    {
        try
        {
            var storageKey = $"{StoragePrefix}{key}";
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", storageKey, value);
        }
        catch
        {
            // 忽略错误
        }
    }

    public async Task<bool> ContainsKeyAsync(string key)
    {
        try
        {
            var storageKey = $"{StoragePrefix}{key}";
            var value = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", storageKey);
            return value != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            var storageKey = $"{StoragePrefix}{key}";
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", storageKey);
        }
        catch
        {
            // 忽略错误
        }
    }

    public async Task ClearAsync()
    {
        try
        {
            // 只清除我们的前缀项
            await _jsRuntime.InvokeVoidAsync("eval",
                $"Object.keys(localStorage).filter(key => key.startsWith('{StoragePrefix}')).forEach(key => localStorage.removeItem(key))");
        }
        catch
        {
            // 忽略错误
        }
    }
}