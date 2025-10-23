using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.Domain;

public interface IResourceStorageStrategy
{
    string Name { get; }
    int Priority { get; }
    Task<bool> InitializeAsync();
    Task<IEnumerable<LanguageResource>> LoadResourcesAsync(string culture);
    Task<bool> SaveResourcesAsync(string culture, IEnumerable<LanguageResource> resources);
    Task<bool> SupportsCultureAsync(string culture);
    Task<IEnumerable<string>> GetSupportedCulturesAsync();
}

// 基础策略抽象类 (Template Method Pattern)
public abstract class BaseResourceStorageStrategy : IResourceStorageStrategy
{
    public abstract string Name { get; }
    public abstract int Priority { get; }

    protected readonly ILogger _logger;
    protected readonly IFileSystem _fileSystem;

    protected BaseResourceStorageStrategy(ILogger logger, IFileSystem fileSystem)
    {
        _logger = logger;
        _fileSystem = fileSystem;
    }

    // Template Method - 定义算法骨架
    public virtual async Task<bool> InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Initializing {StrategyName} storage strategy", Name);
            return await PerformInitializationAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize {StrategyName} storage strategy", Name);
            return false;
        }
    }

    protected abstract Task<bool> PerformInitializationAsync();
    public abstract Task<IEnumerable<LanguageResource>> LoadResourcesAsync(string culture);
    public abstract Task<bool> SaveResourcesAsync(string culture, IEnumerable<LanguageResource> resources);
    public abstract Task<bool> SupportsCultureAsync(string culture);
    public abstract Task<IEnumerable<string>> GetSupportedCulturesAsync();

    protected virtual Dictionary<string, string> GetBuiltInResources(string culture)
    {
        var resources = new Dictionary<string, Dictionary<string, string>>
        {
            ["zh-CN"] = new()
            {
                ["nav.home"] = "首页",
                ["nav.products"] = "产品",
                ["nav.about"] = "关于我们",
                ["nav.contact"] = "联系我们",
                ["action.login"] = "登录",
                ["action.register"] = "注册",
                ["logo.text"] = "流程锻造",
                ["language.english"] = "English",
                ["language.chinese"] = "中文",
                ["language.spanish"] = "Español",
                ["footer.copyright"] = "© 2024 流程锻造平台. 保留所有权利。",
                ["menu.toggle"] = "切换菜单",
                ["error.not_found"] = "未找到资源: {0}",
                ["validation.required"] = "{0} 是必填字段"
            },
            ["en"] = new()
            {
                ["nav.home"] = "Home",
                ["nav.products"] = "Products",
                ["nav.about"] = "About Us",
                ["nav.contact"] = "Contact",
                ["action.login"] = "Login",
                ["action.register"] = "Register",
                ["logo.text"] = "Flow Forge",
                ["language.english"] = "English",
                ["language.chinese"] = "中文",
                ["language.spanish"] = "Español",
                ["footer.copyright"] = "© 2024 Flow Forge Platform. All rights reserved.",
                ["menu.toggle"] = "Toggle Menu",
                ["error.not_found"] = "Resource not found: {0}",
                ["validation.required"] = "{0} is required"
            }
        };

        return resources.ContainsKey(culture) ? resources[culture] : new Dictionary<string, string>();
    }
}