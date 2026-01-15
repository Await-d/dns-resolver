namespace DnsResolver.Domain.Aggregates.DdnsTask;

public interface IDdnsTaskRepository
{
    Task<DdnsTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DdnsTask>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DdnsTask>> GetEnabledTasksAsync(CancellationToken cancellationToken = default);
    Task<DdnsTask> AddAsync(DdnsTask task, CancellationToken cancellationToken = default);
    Task UpdateAsync(DdnsTask task, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
