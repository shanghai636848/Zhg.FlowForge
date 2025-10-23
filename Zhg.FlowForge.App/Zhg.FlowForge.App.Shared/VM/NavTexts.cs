using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.App.Shared.VM;

public class NavItem
{
    public string TextResourceKey { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class ActionButton
{
    public string TextResourceKey { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool IsPrimary { get; set; } = false;
}