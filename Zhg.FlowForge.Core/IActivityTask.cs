namespace Zhg.FlowForge.Core;


/// <summary>
/// 输入数据标记接口
/// </summary>
public interface IInput;

/// <summary>
/// 输出数据标记接口
/// </summary>
public interface IOutput;

/// <summary>
/// 任务执行接口 - 使用具体类型替代接口
/// </summary>
public interface IActivityTask<TInput, TOutput>
    where TInput : IInput, new()
    where TOutput : IOutput, new()
{
    /// <summary>任务输入数据</summary>
    TInput Input { get; set; }

    /// <summary>任务输出数据</summary>
    TOutput Output { get; }

    /// <summary>异步执行任务</summary>
    ValueTask ExecuteAsync(CancellationToken cancellationToken = default);
}