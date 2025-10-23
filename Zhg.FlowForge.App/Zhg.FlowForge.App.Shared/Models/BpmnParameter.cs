using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.App.Shared.Models;

public class BpmnParameter
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public string Value { get; set; } = string.Empty;

    public static BpmnParameter FromJsObject(object jsObject)
    {
        // 这里需要根据实际的 JavaScript 对象结构来解析
        // 这是一个简化的实现
        return new BpmnParameter
        {
            Name = GetProperty(jsObject, "name") ?? $"param_{DateTime.Now.Ticks}",
            Type = GetProperty(jsObject, "type") ?? "string",
            Value = GetProperty(jsObject, "value") ?? GetProperty(jsObject, "$body") ?? string.Empty
        };
    }

    private static string? GetProperty(object obj, string propertyName)
    {
        try
        {
            var property = obj.GetType().GetProperty(propertyName);
            return property?.GetValue(obj)?.ToString();
        }
        catch
        {
            return null;
        }
    }

    public object ToJsObject()
    {
        return new
        {
            name = Name,
            type = Type,
            value = Value
        };
    }
}
