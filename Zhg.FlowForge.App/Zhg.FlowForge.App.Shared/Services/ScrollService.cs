using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Zhg.FlowForge.App.Shared.Services;

public class ScrollService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<ScrollService>? _dotnetHelper;
    private Action<double>? _onScrollChanged;

    public double CurrentScrollY { get; private set; }

    public event Action<double> OnScrollChanged
    {
        add => _onScrollChanged += value;
        remove => _onScrollChanged -= value;
    }

    public ScrollService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitializeAsync()
    {
        if (_jsModule is not null) return; 

        try
        {
            _jsModule = await _js.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Zhg.FlowForge.App.Shared/js/scroll-service.js");

            _dotnetHelper = DotNetObjectReference.Create(this);
            await _jsModule.InvokeVoidAsync("initScrollService", _dotnetHelper);
        }
        catch (JSException ex)
        {
            Debug.WriteLine($"ScrollService init failed: {ex.Message}");
        }
    }

    [JSInvokable]
    public void OnScroll(double y)
    {
        CurrentScrollY = y;
        _onScrollChanged?.Invoke(y);
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsModule is not null)
        {
            try
            {
                await _jsModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Blazor Server: circuit 已断开，忽略
            }
            _jsModule = null;
        }
        _dotnetHelper?.Dispose();
        _dotnetHelper = null;
    }
}