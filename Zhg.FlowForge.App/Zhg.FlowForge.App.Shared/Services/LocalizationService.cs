using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.App.Shared.Services;

public interface ILocalizationService
{
    event Action? OnLanguageChanged;
    string CurrentLanguage { get; }
    List<LanguageInfo> AvailableLanguages { get; }
    Task SetLanguageAsync(string languageCode);
    string Translate(string key);
    string this[string key] { get; }
}

public record LanguageInfo(string Code, string Name, string NativeName, string Flag);

public class LocalizationService : ILocalizationService
{
    private string _currentLanguage = "zh-CN";
    private readonly Dictionary<string, Dictionary<string, string>> _translations = [];

    public event Action? OnLanguageChanged;

    public string CurrentLanguage => _currentLanguage;

    public List<LanguageInfo> AvailableLanguages { get; } =
    [
        new("zh-CN", "Chinese", "简体中文", "🇨🇳"),
        new("zh-TW", "Chinese Traditional", "繁體中文", "🇹🇼"),
        new("en-US", "English", "English", "🇺🇸"),
        new("ja-JP", "Japanese", "日本語", "🇯🇵"),
        new("ko-KR", "Korean", "한국어", "🇰🇷"),
        new("es-ES", "Spanish", "Español", "🇪🇸"),
        new("fr-FR", "French", "Français", "🇫🇷"),
        new("de-DE", "German", "Deutsch", "🇩🇪")
    ];

    public LocalizationService()
    {
        InitializeTranslations();
    }

    public async Task SetLanguageAsync(string languageCode)
    {
        if (_translations.ContainsKey(languageCode))
        {
            _currentLanguage = languageCode;
            OnLanguageChanged?.Invoke();

            // 保存到 localStorage
            await Task.CompletedTask;
        }
    }

    public string Translate(string key)
    {
        if (_translations.TryGetValue(_currentLanguage, out var translations))
        {
            if (translations.TryGetValue(key, out var value))
            {
                return value;
            }
        }
        return key; // 返回 key 作为后备
    }

    public string this[string key] => Translate(key);

    private void InitializeTranslations()
    {
        // 简体中文
        _translations["zh-CN"] = new Dictionary<string, string>
        {
            // 通用
            ["common.home"] = "首页",
            ["common.designer"] = "流程设计",
            ["common.editor"] = "代码编辑",
            ["common.templates"] = "模板库",
            ["common.projects"] = "我的项目",
            ["common.docs"] = "文档",
            ["common.help"] = "帮助",
            ["common.settings"] = "设置",
            ["common.profile"] = "个人资料",
            ["common.logout"] = "退出登录",
            ["common.login"] = "登录",
            ["common.register"] = "注册",

            // 操作
            ["action.save"] = "保存",
            ["action.cancel"] = "取消",
            ["action.delete"] = "删除",
            ["action.edit"] = "编辑",
            ["action.create"] = "创建",
            ["action.search"] = "搜索",
            ["action.filter"] = "筛选",
            ["action.export"] = "导出",
            ["action.import"] = "导入",
            ["action.upload"] = "上传",
            ["action.download"] = "下载",
            ["action.submit"] = "提交",
            ["action.confirm"] = "确认",
            ["action.close"] = "关闭",

            // 导航
            ["nav.welcome"] = "欢迎回来",
            ["nav.notifications"] = "通知",
            ["nav.search.placeholder"] = "搜索...",
            ["nav.theme"] = "主题",
            ["nav.language"] = "语言",

            // 项目
            ["project.new"] = "新建项目",
            ["project.import"] = "导入项目",
            ["project.recent"] = "最近使用",
            ["project.all"] = "全部项目",
            ["project.name"] = "项目名称",
            ["project.description"] = "项目描述",
            ["project.status"] = "状态",
            ["project.created"] = "创建时间",
            ["project.updated"] = "更新时间",

            // 状态
            ["status.developing"] = "开发中",
            ["status.completed"] = "已完成",
            ["status.deployed"] = "已部署",
            ["status.archived"] = "已归档",

            // 页脚
            ["footer.company"] = "© 2025 FlowForge. 保留所有权利。",
            ["footer.about"] = "关于我们",
            ["footer.contact"] = "联系我们",
            ["footer.privacy"] = "隐私政策",
            ["footer.terms"] = "服务条款",
            ["footer.resources"] = "资源",
            ["footer.documentation"] = "文档中心",
            ["footer.api"] = "API 参考",
            ["footer.community"] = "社区",
            ["footer.support"] = "技术支持",
            ["footer.feedback"] = "反馈建议",
            ["footer.social"] = "社交媒体",

            // 消息
            ["message.success"] = "操作成功",
            ["message.error"] = "操作失败",
            ["message.loading"] = "加载中...",
            ["message.saving"] = "保存中...",
            ["message.deleting"] = "删除中...",
            ["message.no_data"] = "暂无数据",
            ["message.confirm_delete"] = "确认删除此项？",
        };

        // 英文
        _translations["en-US"] = new Dictionary<string, string>
        {
            // Common
            ["common.home"] = "Home",
            ["common.designer"] = "Designer",
            ["common.editor"] = "CodeEditor",
            ["common.templates"] = "Templates",
            ["common.projects"] = "Projects",
            ["common.docs"] = "Docs",
            ["common.help"] = "Help",
            ["common.settings"] = "Settings",
            ["common.profile"] = "Profile",
            ["common.logout"] = "Logout",
            ["common.login"] = "Login",
            ["common.register"] = "Register",

            // Actions
            ["action.save"] = "Save",
            ["action.cancel"] = "Cancel",
            ["action.delete"] = "Delete",
            ["action.edit"] = "Edit",
            ["action.create"] = "Create",
            ["action.search"] = "Search",
            ["action.filter"] = "Filter",
            ["action.export"] = "Export",
            ["action.import"] = "Import",
            ["action.upload"] = "Upload",
            ["action.download"] = "Download",
            ["action.submit"] = "Submit",
            ["action.confirm"] = "Confirm",
            ["action.close"] = "Close",

            // Navigation
            ["nav.welcome"] = "Welcome back",
            ["nav.notifications"] = "Notifications",
            ["nav.search.placeholder"] = "Search...",
            ["nav.theme"] = "Theme",
            ["nav.language"] = "Language",

            // Project
            ["project.new"] = "New Project",
            ["project.import"] = "Import Project",
            ["project.recent"] = "Recent",
            ["project.all"] = "All Projects",
            ["project.name"] = "Project Name",
            ["project.description"] = "Description",
            ["project.status"] = "Status",
            ["project.created"] = "Created",
            ["project.updated"] = "Updated",

            // Status
            ["status.developing"] = "Developing",
            ["status.completed"] = "Completed",
            ["status.deployed"] = "Deployed",
            ["status.archived"] = "Archived",

            // Footer
            ["footer.company"] = "© 2025 FlowForge. All rights reserved.",
            ["footer.about"] = "About Us",
            ["footer.contact"] = "Contact",
            ["footer.privacy"] = "Privacy Policy",
            ["footer.terms"] = "Terms of Service",
            ["footer.resources"] = "Resources",
            ["footer.documentation"] = "Documentation",
            ["footer.api"] = "API Reference",
            ["footer.community"] = "Community",
            ["footer.support"] = "Support",
            ["footer.feedback"] = "Feedback",
            ["footer.social"] = "Social Media",

            // Messages
            ["message.success"] = "Success",
            ["message.error"] = "Error",
            ["message.loading"] = "Loading...",
            ["message.saving"] = "Saving...",
            ["message.deleting"] = "Deleting...",
            ["message.no_data"] = "No data available",
            ["message.confirm_delete"] = "Confirm deletion?",
        };

        // 日文
        _translations["ja-JP"] = new Dictionary<string, string>
        {
            ["common.home"] = "ホーム",
            ["common.designer"] = "デザイナー",
            ["common.templates"] = "テンプレート",
            ["common.projects"] = "プロジェクト",
            ["common.docs"] = "ドキュメント",
            ["common.help"] = "ヘルプ",
            ["common.settings"] = "設定",
            ["common.profile"] = "プロフィール",
            ["common.logout"] = "ログアウト",
            ["common.login"] = "ログイン",
            ["common.register"] = "登録",

            ["action.save"] = "保存",
            ["action.cancel"] = "キャンセル",
            ["action.delete"] = "削除",
            ["action.edit"] = "編集",
            ["action.create"] = "作成",
            ["action.search"] = "検索",

            ["nav.welcome"] = "おかえりなさい",
            ["nav.notifications"] = "通知",
            ["nav.search.placeholder"] = "検索...",
            ["nav.theme"] = "テーマ",
            ["nav.language"] = "言語",

            ["footer.company"] = "© 2025 FlowForge. 全著作権所有。",
        };
    }
}