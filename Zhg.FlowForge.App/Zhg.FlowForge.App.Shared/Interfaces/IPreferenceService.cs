using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.App.Shared.Interfaces;

public interface IPreferenceService
{
    Task<string> GetAsync(string key, string defaultValue);
    Task SetAsync(string key, string value);
    Task<bool> ContainsKeyAsync(string key);
    Task RemoveAsync(string key);
    Task ClearAsync();
}
