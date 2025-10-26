using System;
using System.Collections.Generic;
using System.Text;

namespace Zhg.FlowForge.Application.Contract;

/// <summary>
/// BPMN 流程应用服务接口
/// </summary>
public interface IBpmnProcessService
{
    Task<List<BpmnProcessDto>> GetListAsync(CancellationToken cancellationToken = default);
    Task<BpmnProcessDto?> GetAsync(string id, CancellationToken cancellationToken = default);
    Task<BpmnProcessDto> CreateAsync(CreateBpmnProcessDto input, CancellationToken cancellationToken = default);
    Task<BpmnProcessDto> UpdateAsync(string id, UpdateBpmnProcessDto input, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<BpmnValidationResultDto> ValidateAsync(string id, CancellationToken cancellationToken = default);
    Task<string> ExportToXmlAsync(string id, CancellationToken cancellationToken = default);
    Task<BpmnProcessDto> ImportFromXmlAsync(string xml, CancellationToken cancellationToken = default);
}