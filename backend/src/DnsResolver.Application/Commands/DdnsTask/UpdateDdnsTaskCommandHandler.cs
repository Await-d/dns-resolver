namespace DnsResolver.Application.Commands.DdnsTask;

using DnsResolver.Domain.Aggregates.DdnsTask;
using Microsoft.Extensions.Logging;

public class UpdateDdnsTaskCommandHandler
{
    private readonly IDdnsTaskRepository _repository;
    private readonly ILogger<UpdateDdnsTaskCommandHandler> _logger;

    public UpdateDdnsTaskCommandHandler(
        IDdnsTaskRepository repository,
        ILogger<UpdateDdnsTaskCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<UpdateDdnsTaskResult> HandleAsync(
        UpdateDdnsTaskCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var task = await _repository.GetByIdAsync(command.TaskId, cancellationToken);
            if (task == null)
                return new UpdateDdnsTaskResult(false, "Task not found");

            if (command.Enabled.HasValue)
            {
                if (command.Enabled.Value)
                    task.Enable();
                else
                    task.Disable();
            }

            if (command.IntervalMinutes.HasValue)
                task.UpdateInterval(command.IntervalMinutes.Value);

            if (!string.IsNullOrEmpty(command.ProviderId) && !string.IsNullOrEmpty(command.ProviderSecret))
                task.UpdateCredentials(command.ProviderId, command.ProviderSecret);

            await _repository.UpdateAsync(task, cancellationToken);

            _logger.LogInformation("Updated DDNS task: {TaskId}", command.TaskId);

            return new UpdateDdnsTaskResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update DDNS task: {TaskId}", command.TaskId);
            return new UpdateDdnsTaskResult(false, ex.Message);
        }
    }
}
