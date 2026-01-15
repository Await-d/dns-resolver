namespace DnsResolver.Infrastructure.Repositories;

using DnsResolver.Domain.Aggregates.DdnsTask;
using System.Collections.Concurrent;

public class InMemoryDdnsTaskRepository : IDdnsTaskRepository
{
    private readonly ConcurrentDictionary<Guid, DdnsTask> _tasks = new();

    public Task<DdnsTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _tasks.TryGetValue(id, out var task);
        return Task.FromResult(task);
    }

    public Task<IReadOnlyList<DdnsTask>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tasks = _tasks.Values.OrderByDescending(t => t.CreatedAt).ToList();
        return Task.FromResult<IReadOnlyList<DdnsTask>>(tasks);
    }

    public Task<IReadOnlyList<DdnsTask>> GetEnabledTasksAsync(CancellationToken cancellationToken = default)
    {
        var tasks = _tasks.Values
            .Where(t => t.Enabled)
            .OrderByDescending(t => t.CreatedAt)
            .ToList();
        return Task.FromResult<IReadOnlyList<DdnsTask>>(tasks);
    }

    public Task<DdnsTask> AddAsync(DdnsTask task, CancellationToken cancellationToken = default)
    {
        _tasks[task.Id] = task;
        return Task.FromResult(task);
    }

    public Task UpdateAsync(DdnsTask task, CancellationToken cancellationToken = default)
    {
        _tasks[task.Id] = task;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _tasks.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
