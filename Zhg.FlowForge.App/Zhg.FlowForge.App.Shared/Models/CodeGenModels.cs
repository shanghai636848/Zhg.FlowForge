//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Zhg.FlowForge.App.Shared.Models;

//// 项目配置
//public class ProjectConfig
//{
//    public string ProjectName { get; set; } = "";
//    public string Namespace { get; set; } = "";
//    public string Version { get; set; } = "1.0.0";
//    public string Description { get; set; } = "";
//    public string TargetFramework { get; set; } = "net10.0";
//    public bool EnableNullable { get; set; } = true;
//    public bool ImplicitUsings { get; set; } = true;
//    public string Author { get; set; } = "";
//    public string Company { get; set; } = "";
//    public string Copyright { get; set; } = "";
//}

//// 代码生成选项
//public class CodeGenerationOptions
//{
//    public string NamingStyle { get; set; } = "pascalCase";
//    public string ClassPrefix { get; set; } = "";
//    public string InterfacePrefix { get; set; } = "I";
//    public bool UseFileScoped { get; set; } = true;
//    public bool UseRecordTypes { get; set; } = true;
//    public bool UseExpressionBodies { get; set; } = true;
//    public bool UsePatternMatching { get; set; } = true;
//    public bool GenerateAsyncMethods { get; set; } = true;
//    public bool UseConfigureAwait { get; set; } = false;
//    public bool UseValueTask { get; set; } = false;
//    public bool GenerateLogging { get; set; } = true;
//    public string LoggingFramework { get; set; } = "microsoft";
//    public bool GenerateExceptionHandling { get; set; } = true;
//    public bool UseCustomExceptions { get; set; } = false;
//    public bool GenerateXmlComments { get; set; } = true;
//    public bool IncludeExamples { get; set; } = false;
//    public bool EnableAotOptimizations { get; set; } = false;
//    public bool EnableTrimming { get; set; } = false;
//    public bool GenerateSourceGenerators { get; set; } = false;
//}

//// 包依赖
//public class PackageDependency
//{
//    public string PackageId { get; set; } = "";
//    public string Version { get; set; } = "";
//    public string Description { get; set; } = "";
//    public double Size { get; set; } // MB
//    public bool IsRequired { get; set; }
//}

//// 项目文件
//public class ProjectFile
//{
//    public string Path { get; set; } = "";
//    public string Name { get; set; } = "";
//    public bool IsFolder { get; set; }
//    public bool IsDirty { get; set; }
//    public int LineCount { get; set; }
//    public long Size { get; set; }
//}

//// 代码符号
//public class CodeSymbol
//{
//    public string Name { get; set; } = "";
//    public string Kind { get; set; } = ""; // Class, Method, Property, Field
//    public int Line { get; set; }
//    public int Column { get; set; }
//}

//// 诊断信息
//public class Diagnostic
//{
//    public string Severity { get; set; } = ""; // Error, Warning, Info
//    public string Code { get; set; } = "";
//    public string Message { get; set; } = "";
//    public string File { get; set; } = "";
//    public int Line { get; set; }
//    public int Column { get; set; }
//}

//// 编译结果
//public class CompilationResult
//{
//    public bool Success { get; set; }
//    public string AssemblyName { get; set; } = "";
//    public string OutputPath { get; set; } = "";
//    public long AssemblySize { get; set; }
//    public List<Diagnostic> Diagnostics { get; set; } = new();
//    public int ErrorCount => Diagnostics.Count(d => d.Severity == "Error");
//    public int WarningCount => Diagnostics.Count(d => d.Severity == "Warning");
//    public DateTime StartTime { get; set; }
//    public DateTime EndTime { get; set; }
//}

//// 编译选项
//public class CompilationOptions
//{
//    public string Configuration { get; set; } = "Debug";
//    public string Platform { get; set; } = "AnyCPU";
//    public bool TreatWarningsAsErrors { get; set; }
//}

//// 生成请求
//public class GenerationRequest
//{
//    public string ProcessId { get; set; } = "";
//    public ProjectConfig Config { get; set; } = new();
//    public string Template { get; set; } = "";
//    public CodeGenerationOptions Options { get; set; } = new();
//    public List<PackageDependency> Dependencies { get; set; } = new();
//}

//// 生成结果
//public class GenerationResult
//{
//    public bool Success { get; set; }
//    public string Error { get; set; } = "";
//    public List<GeneratedFile> Files { get; set; } = new();
//    public int TotalLines { get; set; }
//}

//public class GeneratedFile
//{
//    public string Path { get; set; } = "";
//    public string Content { get; set; } = "";
//}