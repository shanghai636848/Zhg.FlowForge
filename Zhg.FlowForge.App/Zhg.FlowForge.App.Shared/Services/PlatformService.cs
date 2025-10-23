using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.App.Shared.Services;

public interface IPlatformService
{
    bool IsMaui { get; }
    bool IsBlazorServer { get; }
    bool IsBlazorWebAssembly { get; }
    string PlatformName { get; }
}

public class PlatformService : IPlatformService
{
    public bool IsMaui
    {
        get
        {
#if ANDROID || IOS || MACCATALYST || WINDOWS
                return true;
#else
            return false;
#endif
        }
    }

    public bool IsBlazorServer
    {
        get
        {
#if SERVER
                return true;
#else
            return false;
#endif
        }
    }

    public bool IsBlazorWebAssembly
    {
        get
        {
#if WASM
                return true;
#else
            return false;
#endif
        }
    }

    public string PlatformName
    {
        get
        {
            if (IsMaui)
            {
#if ANDROID
                    return "Android";
#elif IOS
                    return "iOS";
#elif MACCATALYST
                    return "MacCatalyst";
#elif WINDOWS
                    return "Windows";
#else
                return "MAUI";
#endif
            }
            else if (IsBlazorServer)
            {
                return "Blazor Server";
            }
            else if (IsBlazorWebAssembly)
            {
                return "Blazor WebAssembly";
            }
            else
            {
                return "Unknown";
            }
        }
    }
}