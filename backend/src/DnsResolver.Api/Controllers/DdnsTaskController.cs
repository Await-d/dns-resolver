namespace DnsResolver.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using DnsResolver.Api.Responses;
using DnsResolver.Application.Commands.DdnsTask;
using DnsResolver.Application.Queries.GetDdnsTasks;

[ApiController]
[Route("api/v1/ddns/tasks")]
public class DdnsTaskController : ControllerBase
{
    private readonly CreateDdnsTaskCommandHandler _createHandler;
    private readonly UpdateDdnsTaskCommandHandler _updateHandler;
    private readonly DeleteDdnsTaskCommandHandler _deleteHandler;
    private readonly GetDdnsTasksQueryHandler _getTasksHandler;
    private readonly ILogger<DdnsTaskController> _logger;

    public DdnsTaskController(
        CreateDdnsTaskCommandHandler createHandler,
        UpdateDdnsTaskCommandHandler updateHandler,
        DeleteDdnsTaskCommandHandler deleteHandler,
        GetDdnsTasksQueryHandler getTasksHandler,
        ILogger<DdnsTaskController> logger)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _getTasksHandler = getTasksHandler;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有 DDNS 任务
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DdnsTaskDto>>>> GetTasks(
        CancellationToken ct)
    {
        var tasks = await _getTasksHandler.HandleAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<DdnsTaskDto>>.Ok(tasks));
    }

    /// <summary>
    /// 创建 DDNS 任务
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreateDdnsTaskResult>>> CreateTask(
        [FromBody] CreateDdnsTaskRequest request,
        CancellationToken ct)
    {
        var command = new CreateDdnsTaskCommand(
            request.Name,
            request.ProviderName,
            request.ProviderId,
            request.ProviderSecret,
            request.Domain,
            request.RecordId,
            request.SubDomain,
            request.Ttl,
            request.IntervalMinutes,
            request.ExtraParams);

        var result = await _createHandler.HandleAsync(command, ct);

        if (!result.Success)
            return BadRequest(ApiResponse<CreateDdnsTaskResult>.Fail(result.ErrorMessage ?? "Failed"));

        return Ok(ApiResponse<CreateDdnsTaskResult>.Ok(result));
    }

    /// <summary>
    /// 更新 DDNS 任务
    /// </summary>
    [HttpPut("{taskId}")]
    public async Task<ActionResult<ApiResponse<UpdateDdnsTaskResult>>> UpdateTask(
        Guid taskId,
        [FromBody] UpdateDdnsTaskRequest request,
        CancellationToken ct)
    {
        var command = new UpdateDdnsTaskCommand(
            taskId,
            request.Enabled,
            request.IntervalMinutes,
            request.ProviderId,
            request.ProviderSecret);

        var result = await _updateHandler.HandleAsync(command, ct);

        if (!result.Success)
            return BadRequest(ApiResponse<UpdateDdnsTaskResult>.Fail(result.ErrorMessage ?? "Failed"));

        return Ok(ApiResponse<UpdateDdnsTaskResult>.Ok(result));
    }

    /// <summary>
    /// 删除 DDNS 任务
    /// </summary>
    [HttpDelete("{taskId}")]
    public async Task<ActionResult<ApiResponse<DeleteDdnsTaskResult>>> DeleteTask(
        Guid taskId,
        CancellationToken ct)
    {
        var command = new DeleteDdnsTaskCommand(taskId);
        var result = await _deleteHandler.HandleAsync(command, ct);

        if (!result.Success)
            return BadRequest(ApiResponse<DeleteDdnsTaskResult>.Fail(result.ErrorMessage ?? "Failed"));

        return Ok(ApiResponse<DeleteDdnsTaskResult>.Ok(result));
    }

    /// <summary>
    /// 启用 DDNS 任务
    /// </summary>
    [HttpPost("{taskId}/enable")]
    public async Task<ActionResult<ApiResponse<UpdateDdnsTaskResult>>> EnableTask(
        Guid taskId,
        CancellationToken ct)
    {
        var command = new UpdateDdnsTaskCommand(taskId, Enabled: true);
        var result = await _updateHandler.HandleAsync(command, ct);

        if (!result.Success)
            return BadRequest(ApiResponse<UpdateDdnsTaskResult>.Fail(result.ErrorMessage ?? "Failed"));

        return Ok(ApiResponse<UpdateDdnsTaskResult>.Ok(result));
    }

    /// <summary>
    /// 禁用 DDNS 任务
    /// </summary>
    [HttpPost("{taskId}/disable")]
    public async Task<ActionResult<ApiResponse<UpdateDdnsTaskResult>>> DisableTask(
        Guid taskId,
        CancellationToken ct)
    {
        var command = new UpdateDdnsTaskCommand(taskId, Enabled: false);
        var result = await _updateHandler.HandleAsync(command, ct);

        if (!result.Success)
            return BadRequest(ApiResponse<UpdateDdnsTaskResult>.Fail(result.ErrorMessage ?? "Failed"));

        return Ok(ApiResponse<UpdateDdnsTaskResult>.Ok(result));
    }
}

// Request models
public record CreateDdnsTaskRequest(
    string Name,
    string ProviderName,
    string ProviderId,
    string ProviderSecret,
    string Domain,
    string RecordId,
    string? SubDomain = null,
    int Ttl = 600,
    int IntervalMinutes = 5,
    Dictionary<string, string>? ExtraParams = null);

public record UpdateDdnsTaskRequest(
    bool? Enabled = null,
    int? IntervalMinutes = null,
    string? ProviderId = null,
    string? ProviderSecret = null);
