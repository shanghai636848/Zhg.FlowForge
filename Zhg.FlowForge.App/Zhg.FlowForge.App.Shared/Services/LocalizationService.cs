

using System.Globalization;

namespace Zhg.FlowForge.App.Shared.Services;

public interface ILocalizationService
{
    CultureInfo CurrentCulture { get; }
    event Action OnLanguageChanged;
    void SetLanguage(string culture);
    string GetString(string key);
    Task LoadLanguageAsync(string culture);
}

public class LocalizationService : ILocalizationService
{
    private readonly Dictionary<string, Dictionary<string, string>> _localizations;
    private CultureInfo _currentCulture;

    public CultureInfo CurrentCulture => _currentCulture;
    public event Action OnLanguageChanged;

    public LocalizationService()
    {
        _localizations = new Dictionary<string, Dictionary<string, string>>();
        _currentCulture = CultureInfo.CurrentCulture;

        // 初始化语言资源
        InitializeResources();
    }

    private void InitializeResources()
    {
        // 英语资源
        _localizations["en"] = new Dictionary<string, string>
        {
            ["nav.home"] = "Home",
            ["nav.products"] = "Products",
            ["nav.about"] = "About Us",
            ["nav.contact"] = "Contact",
            ["action.login"] = "Login",
            ["action.register"] = "Register",
            ["logo.text"] = "MyApp",
            ["language.english"] = "English",
            ["language.chinese"] = "中文"
        };

        // 中文资源
        _localizations["zh-CN"] = new Dictionary<string, string>
        {
            ["nav.home"] = "首页",
            ["nav.products"] = "产品",
            ["nav.about"] = "关于我们",
            ["nav.contact"] = "联系",
            ["action.login"] = "登录",
            ["action.register"] = "注册",
            ["logo.text"] = "我的应用",
            ["language.english"] = "English",
            ["language.chinese"] = "中文"
        };

        // 添加更多语言...
    }

    public void SetLanguage(string culture)
    {
        if (_localizations.ContainsKey(culture))
        {
            _currentCulture = new CultureInfo(culture);
            OnLanguageChanged?.Invoke();
        }
    }

    public async Task LoadLanguageAsync(string culture)
    {
        // 这里可以扩展为从外部文件加载语言资源
        await Task.CompletedTask;
        SetLanguage(culture);
    }

    public string GetString(string key)
    {
        var culture = _currentCulture.Name;
        if (_localizations.ContainsKey(culture) && _localizations[culture].ContainsKey(key))
        {
            return _localizations[culture][key];
        }

        // 回退到英语
        if (_localizations["en"].ContainsKey(key))
        {
            return _localizations["en"][key];
        }

        return key; // 如果找不到，返回键名
    }
}