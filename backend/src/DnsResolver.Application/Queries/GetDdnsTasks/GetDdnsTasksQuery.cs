namespace DnsResolver.Application.Queries.GetDdnsTasks;

public record GetDdnsTasksQuery;

public record DdnsTaskDto(
    Guid Id,
    string Name,
    string ProviderName,
    string Domain,
    string RecordId,
    string? SubDomain,
    int Ttl,
    int IntervalMinutes,
    bool Enabled,
    string? LastKnownIp,
    DateTime? LastCheckTime,
    DateTime? LastUpdateTime,
    string? LastError,
    DateTime CreatedAt,
    DateTime UpdatedAt);
