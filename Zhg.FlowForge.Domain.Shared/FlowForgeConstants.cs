using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.Domain.Shared;

/// <summary>
/// FlowForge 常量
/// </summary>
public static class FlowForgeConstants
{
    public const string DefaultTargetFramework = "net10.0";
    public const string LocalRootPath = "C:\\FlowForge\\Projects";

    public static class Templates
    {
        public const string Standard = "standard";
        public const string Minimal = "minimal";
        public const string Microservice = "microservice";
        public const string Enterprise = "enterprise";
    }

    public static class FileExtensions
    {
        public const string CSharp = ".cs";
        public const string Razor = ".razor";
        public const string Json = ".json";
        public const string Xml = ".xml";
    }
}