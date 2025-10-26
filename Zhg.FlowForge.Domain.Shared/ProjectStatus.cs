using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.Domain.Shared;

/// <summary>
/// 项目状态枚举
/// </summary>
public enum ProjectStatus
{
    Developing = 1,
    Completed = 2,
    Deployed = 3,
    Archived = 4
}

/// <summary>
/// 诊断严重级别
/// </summary>
public enum DiagnosticSeverity
{
    Error = 1,
    Warning = 2,
    Info = 3,
    Hint = 4
}

/// <summary>
/// 代码符号类型
/// </summary>
public enum CodeSymbolKind
{
    Class,
    Interface,
    Method,
    Property,
    Field,
    Event
}

/// <summary>
/// Toast 类型
/// </summary>
public enum ToastType
{
    Success = 1,
    Error = 2,
    Warning = 3,
    Info = 4
}