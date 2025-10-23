using System;
using System.Collections.Generic;
using System.Text;
using Zhg.FlowForge.App.Shared.Models;
using Zhg.FlowForge.App.Shared.Services;

namespace Zhg.FlowForge.App.Shared.Interfaces;

public interface IJsonResourceService
{
    Task<ResourceLoadResult> LoadResourcesAsync(string culture);
    Task<ResourceSaveResult> SaveResourcesAsync(string culture, Dictionary<string, string> resources);
    Task<List<string>> GetAvailableCulturesAsync();
    Task<bool> InitializeAsync();
}
