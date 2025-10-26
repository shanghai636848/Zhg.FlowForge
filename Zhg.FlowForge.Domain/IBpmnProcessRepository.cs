using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.Domain;

/// <summary>
/// BPMN 流程仓储接口
/// </summary>
public interface IBpmnProcessRepository
{
    Task<BpmnProcess?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<List<BpmnProcess>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BpmnProcess> AddAsync(BpmnProcess process, CancellationToken cancellationToken = default);
    Task UpdateAsync(BpmnProcess process, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}