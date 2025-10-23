namespace Zhg.FlowForge.App.Shared.Models
{
    /// <summary>
    /// BPMN 参数模型类
    /// 用于输入/输出参数的数据绑定
    /// </summary>
    public class BpmnParameter
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 参数类型
        /// 支持: string, long, boolean, json
        /// </summary>
        public string Type { get; set; } = "string";

        /// <summary>
        /// 参数值或表达式
        /// 例如: ${execution.getVariable('userId')}
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// 参数描述（可选）
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 是否必填（可选）
        /// </summary>
        public bool IsRequired { get; set; } = false;

        /// <summary>
        /// 创建时间（可选）
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 无参构造函数
        /// </summary>
        public BpmnParameter()
        {
        }

        /// <summary>
        /// 带参数的构造函数
        /// </summary>
        /// <param name="name">参数名</param>
        /// <param name="type">参数类型</param>
        /// <param name="value">参数值</param>
        public BpmnParameter(string name, string type, string value)
        {
            Name = name;
            Type = type;
            Value = value;
        }

        /// <summary>
        /// 从字典创建参数对象
        /// </summary>
        /// <param name="dict">字典数据</param>
        /// <returns>参数对象</returns>
        public static BpmnParameter FromDictionary(Dictionary<string, object> dict)
        {
            return new BpmnParameter
            {
                Name = dict.ContainsKey("name") ? dict["name"]?.ToString() ?? "" : "",
                Type = dict.ContainsKey("type") ? dict["type"]?.ToString() ?? "string" : "string",
                Value = dict.ContainsKey("value") ? dict["value"]?.ToString() ?? "" : ""
            };
        }

        /// <summary>
        /// 转换为字典
        /// </summary>
        /// <returns>字典数据</returns>
        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                { "name", Name },
                { "type", Type },
                { "value", Value }
            };
        }

        /// <summary>
        /// 验证参数是否有效
        /// </summary>
        /// <returns>是否有效</returns>
        public bool IsValid()
        {
            // 参数名不能为空
            if (string.IsNullOrWhiteSpace(Name))
                return false;

            // 类型必须是支持的类型之一
            var supportedTypes = new[] { "string", "long", "boolean", "json" };
            if (!supportedTypes.Contains(Type.ToLower()))
                return false;

            return true;
        }

        /// <summary>
        /// 克隆参数对象
        /// </summary>
        /// <returns>新的参数对象</returns>
        public BpmnParameter Clone()
        {
            return new BpmnParameter
            {
                Name = Name,
                Type = Type,
                Value = Value,
                Description = Description,
                IsRequired = IsRequired,
                CreatedAt = CreatedAt
            };
        }

        /// <summary>
        /// 重写 ToString 方法
        /// </summary>
        /// <returns>字符串表示</returns>
        public override string ToString()
        {
            return $"{Name} ({Type}): {Value}";
        }

        /// <summary>
        /// 重写 Equals 方法
        /// </summary>
        /// <param name="obj">比较对象</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object? obj)
        {
            if (obj is not BpmnParameter other)
                return false;

            return Name == other.Name &&
                   Type == other.Type &&
                   Value == other.Value;
        }

        /// <summary>
        /// 重写 GetHashCode 方法
        /// </summary>
        /// <returns>哈希码</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Type, Value);
        }
    }

    /// <summary>
    /// BPMN 参数类型枚举
    /// </summary>
    public enum BpmnParameterType
    {
        /// <summary>
        /// 字符串类型
        /// </summary>
        String,

        /// <summary>
        /// 长整型
        /// </summary>
        Long,

        /// <summary>
        /// 布尔型
        /// </summary>
        Boolean,

        /// <summary>
        /// JSON 对象
        /// </summary>
        Json
    }

    /// <summary>
    /// BPMN 参数扩展方法
    /// </summary>
    public static class BpmnParameterExtensions
    {
        /// <summary>
        /// 将类型字符串转换为枚举
        /// </summary>
        /// <param name="typeString">类型字符串</param>
        /// <returns>枚举值</returns>
        public static BpmnParameterType ToParameterType(this string typeString)
        {
            return typeString.ToLower() switch
            {
                "string" => BpmnParameterType.String,
                "long" => BpmnParameterType.Long,
                "boolean" => BpmnParameterType.Boolean,
                "json" => BpmnParameterType.Json,
                _ => BpmnParameterType.String
            };
        }

        /// <summary>
        /// 将枚举转换为类型字符串
        /// </summary>
        /// <param name="type">枚举值</param>
        /// <returns>类型字符串</returns>
        public static string ToTypeString(this BpmnParameterType type)
        {
            return type switch
            {
                BpmnParameterType.String => "string",
                BpmnParameterType.Long => "long",
                BpmnParameterType.Boolean => "boolean",
                BpmnParameterType.Json => "json",
                _ => "string"
            };
        }

        /// <summary>
        /// 获取类型的显示名称
        /// </summary>
        /// <param name="type">枚举值</param>
        /// <returns>显示名称</returns>
        public static string GetDisplayName(this BpmnParameterType type)
        {
            return type switch
            {
                BpmnParameterType.String => "String (字符串)",
                BpmnParameterType.Long => "Long (长整型)",
                BpmnParameterType.Boolean => "Boolean (布尔)",
                BpmnParameterType.Json => "JSON (对象)",
                _ => "String (字符串)"
            };
        }
    }
}