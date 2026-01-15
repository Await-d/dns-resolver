namespace DnsResolver.Application.Commands.DdnsTask;

public record CreateDdnsTaskCommand(
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

public record CreateDdnsTaskResult(
    Guid TaskId,
    string Name,
    bool Success,
    string? ErrorMessage = null);
