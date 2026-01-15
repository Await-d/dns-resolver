namespace DnsResolver.Application.Commands.DdnsTask;

using DnsResolver.Domain.Aggregates.DdnsTask;
using Microsoft.Extensions.Logging;

public class DeleteDdnsTaskCommandHandler
{
    private readonly IDdnsTaskRepository _repository;
    private readonly ILogger<DeleteDdnsTaskCommandHandler> _logger;

    public DeleteDdnsTaskCommandHandler(
        IDdnsTaskRepository repository,
        ILogger<DeleteDdnsTaskCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<DeleteDdnsTaskResult> HandleAsync(
        DeleteDdnsTaskCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var task = await _repository.GetByIdAsync(command.TaskId, cancellationToken);
            if (task == null)
                return new DeleteDdnsTaskResult(false, "Task not found");

            await _repository.DeleteAsync(command.TaskId, cancellationToken);

            _logger.LogInformation("Deleted DDNS task: {TaskId}", command.TaskId);

            return new DeleteDdnsTaskResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete DDNS task: {TaskId}", command.TaskId);
            return new DeleteDdnsTaskResult(false, ex.Message);
        }
    }
}
