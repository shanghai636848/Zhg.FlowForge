using System.Collections.Concurrent;
using Zhg.FlowForge.Domain;

namespace Zhg.FlowForge.Api;

/// <summary>
/// 内存 BPMN 流程仓储实现
/// </summary>
public class InMemoryBpmnProcessRepository : IBpmnProcessRepository
{
    private readonly ConcurrentDictionary<string, BpmnProcess> _processes = new();

    public InMemoryBpmnProcessRepository()
    {
        InitializeSampleData();
    }

    public Task<BpmnProcess?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _processes.TryGetValue(id, out var process);
        return Task.FromResult(process);
    }

    public Task<List<BpmnProcess>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_processes.Values.OrderByDescending(p => p.UpdatedAt).ToList());
    }

    public Task<BpmnProcess> AddAsync(BpmnProcess process, CancellationToken cancellationToken = default)
    {
        _processes[process.Id] = process;
        return Task.FromResult(process);
    }

    public Task UpdateAsync(BpmnProcess process, CancellationToken cancellationToken = default)
    {
        _processes[process.Id] = process;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        _processes.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    private void InitializeSampleData()
    {
        // 示例流程 1: 订单处理
        var orderProcess = BpmnProcess.Create("订单处理流程", "电商订单自动化处理");

        orderProcess.AddActivity(BpmnActivity.Create("接收订单", "UserTask"));
        orderProcess.AddActivity(BpmnActivity.Create("验证订单", "ServiceTask"));
        orderProcess.AddActivity(BpmnActivity.Create("检查库存", "ServiceTask"));
        orderProcess.AddActivity(BpmnActivity.Create("处理支付", "ServiceTask"));
        orderProcess.AddActivity(BpmnActivity.Create("发货", "ServiceTask"));
        orderProcess.AddActivity(BpmnActivity.Create("通知客户", "SendTask"));

        orderProcess.AddSequenceFlow(BpmnSequenceFlow.Create("start", "接收订单"));
        orderProcess.AddSequenceFlow(BpmnSequenceFlow.Create("接收订单", "验证订单"));
        orderProcess.AddSequenceFlow(BpmnSequenceFlow.Create("验证订单", "检查库存"));
        orderProcess.AddSequenceFlow(BpmnSequenceFlow.Create("检查库存", "处理支付"));
        orderProcess.AddSequenceFlow(BpmnSequenceFlow.Create("处理支付", "发货"));
        orderProcess.AddSequenceFlow(BpmnSequenceFlow.Create("发货", "通知客户"));
        orderProcess.AddSequenceFlow(BpmnSequenceFlow.Create("通知客户", "end"));

        _processes[orderProcess.Id] = orderProcess;

        // 示例流程 2: 审批流程
        var approvalProcess = BpmnProcess.Create("审批流程", "多级审批工作流");

        approvalProcess.AddActivity(BpmnActivity.Create("提交申请", "UserTask"));
        approvalProcess.AddActivity(BpmnActivity.Create("经理审批", "UserTask"));
        approvalProcess.AddActivity(BpmnActivity.Create("总监审批", "UserTask"));
        approvalProcess.AddActivity(BpmnActivity.Create("批准", "ServiceTask"));
        approvalProcess.AddActivity(BpmnActivity.Create("拒绝", "ServiceTask"));

        approvalProcess.AddGateway(BpmnGateway.Create("审批决策", "Exclusive"));

        approvalProcess.AddSequenceFlow(BpmnSequenceFlow.Create("start", "提交申请"));
        approvalProcess.AddSequenceFlow(BpmnSequenceFlow.Create("提交申请", "经理审批"));
        approvalProcess.AddSequenceFlow(BpmnSequenceFlow.Create("经理审批", "总监审批"));
        approvalProcess.AddSequenceFlow(BpmnSequenceFlow.Create("总监审批", "审批决策"));
        approvalProcess.AddSequenceFlow(BpmnSequenceFlow.Create("审批决策", "批准", "approved == true"));
        approvalProcess.AddSequenceFlow(BpmnSequenceFlow.Create("审批决策", "拒绝", "approved == false"));
        approvalProcess.AddSequenceFlow(BpmnSequenceFlow.Create("批准", "end"));
        approvalProcess.AddSequenceFlow(BpmnSequenceFlow.Create("拒绝", "end"));

        _processes[approvalProcess.Id] = approvalProcess;
    }
}