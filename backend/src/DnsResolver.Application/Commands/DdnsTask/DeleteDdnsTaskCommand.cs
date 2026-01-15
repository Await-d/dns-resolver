namespace DnsResolver.Application.Commands.DdnsTask;

public record DeleteDdnsTaskCommand(Guid TaskId);

public record DeleteDdnsTaskResult(bool Success, string? ErrorMessage = null);
