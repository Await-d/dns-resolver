namespace DnsResolver.Application.Queries.GetDdnsTasks;

using DnsResolver.Domain.Aggregates.DdnsTask;

public class GetDdnsTasksQueryHandler
{
    private readonly IDdnsTaskRepository _repository;

    public GetDdnsTasksQueryHandler(IDdnsTaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<DdnsTaskDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var tasks = await _repository.GetAllAsync(cancellationToken);

        return tasks.Select(t => new DdnsTaskDto(
            t.Id,
            t.Name,
            t.ProviderName,
            t.Domain,
            t.RecordId,
            t.SubDomain,
            t.Ttl,
            t.IntervalMinutes,
            t.Enabled,
            t.LastKnownIp,
            t.LastCheckTime,
            t.LastUpdateTime,
            t.LastError,
            t.CreatedAt,
            t.UpdatedAt
        )).ToList();
    }
}
