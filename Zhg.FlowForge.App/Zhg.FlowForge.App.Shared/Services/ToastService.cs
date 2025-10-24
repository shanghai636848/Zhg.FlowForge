using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.App.Shared.Services;

public interface IToastService
{
    event Action<ToastMessage>? OnShow;
    void Success(string message, string? title = null);
    void Error(string message, string? title = null); void Warning(string message, string? title = null);
    void Info(string message, string? title = null);
}
public class ToastService : IToastService
{
    public event Action<ToastMessage>? OnShow;
    public void Success(string message, string? title = null)
    {
        Show(new ToastMessage
        {
            Id = Guid.NewGuid().ToString(),
            Type = ToastType.Success,
            Message = message,
            Title = title,
            Duration = 3000
        });
    }

    public void Error(string message, string? title = null)
    {
        Show(new ToastMessage
        {
            Id = Guid.NewGuid().ToString(),
            Type = ToastType.Error,
            Message = message,
            Title = title,
            Duration = 5000
        });
    }

    public void Warning(string message, string? title = null)
    {
        Show(new ToastMessage
        {
            Id = Guid.NewGuid().ToString(),
            Type = ToastType.Warning,
            Message = message,
            Title = title,
            Duration = 4000
        });
    }

    public void Info(string message, string? title = null)
    {
        Show(new ToastMessage
        {
            Id = Guid.NewGuid().ToString(),
            Type = ToastType.Info,
            Message = message,
            Title = title,
            Duration = 3000
        });
    }

    private void Show(ToastMessage message)
    {
        OnShow?.Invoke(message);
    }
}
public class ToastMessage
{
    public string Id { get; set; } = "";
    public ToastType Type { get; set; }
    public string Message { get; set; } = "";
    public string? Title { get; set; }
    public int Duration { get; set; } = 3000;
}
public enum ToastType
{
    Success,
    Error,
    Warning,
    Info
}