namespace DnsResolver.Application.Commands.DdnsTask;

public record UpdateDdnsTaskCommand(
    Guid TaskId,
    bool? Enabled = null,
    int? IntervalMinutes = null,
    string? ProviderId = null,
    string? ProviderSecret = null);

public record UpdateDdnsTaskResult(bool Success, string? ErrorMessage = null);
