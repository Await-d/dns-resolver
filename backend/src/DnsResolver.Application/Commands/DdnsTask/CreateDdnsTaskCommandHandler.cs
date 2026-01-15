namespace DnsResolver.Application.Commands.DdnsTask;

using DnsResolver.Domain.Aggregates.DdnsTask;
using Microsoft.Extensions.Logging;

public class CreateDdnsTaskCommandHandler
{
    private readonly IDdnsTaskRepository _repository;
    private readonly ILogger<CreateDdnsTaskCommandHandler> _logger;

    public CreateDdnsTaskCommandHandler(
        IDdnsTaskRepository repository,
        ILogger<CreateDdnsTaskCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<CreateDdnsTaskResult> HandleAsync(
        CreateDdnsTaskCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var task = Domain.Aggregates.DdnsTask.DdnsTask.Create(
                command.Name,
                command.ProviderName,
                command.ProviderId,
                command.ProviderSecret,
                command.Domain,
                command.RecordId,
                command.SubDomain,
                command.Ttl,
                command.IntervalMinutes,
                command.ExtraParams);

            await _repository.AddAsync(task, cancellationToken);

            _logger.LogInformation(
                "Created DDNS task: {TaskId} - {Name} for {Domain}",
                task.Id, task.Name, task.Domain);

            return new CreateDdnsTaskResult(task.Id, task.Name, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create DDNS task: {Name}", command.Name);
            return new CreateDdnsTaskResult(Guid.Empty, command.Name, false, ex.Message);
        }
    }
}
