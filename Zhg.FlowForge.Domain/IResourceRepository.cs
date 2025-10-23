using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.Domain;

public interface IResourceRepository : IDisposable
{
    Task InitializeAsync();
    Task<LanguageResource?> GetAsync(string key, string culture);
    Task<IEnumerable<LanguageResource>> GetByCultureAsync(string culture);
    Task<IEnumerable<LanguageResource>> GetByModuleAsync(string module, string culture);
    Task<bool> AddOrUpdateAsync(LanguageResource resource);
    Task<bool> AddOrUpdateRangeAsync(IEnumerable<LanguageResource> resources);
    Task<bool> RemoveAsync(string key, string culture);
    Task<IEnumerable<string>> GetAvailableCulturesAsync();
    Task<int> GetResourceCountAsync(string culture);
    Task<bool> ExistsAsync(string key, string culture);
}

public interface IResourceQuery
{
    IResourceQuery WithCulture(string culture);
    IResourceQuery WithModule(string module);
    IResourceQuery WithKey(string key);
    IResourceQuery WithType(ResourceType type);
    Task<IEnumerable<LanguageResource>> ExecuteAsync();
    Task<LanguageResource?> FirstOrDefaultAsync();
    Task<int> CountAsync();
}