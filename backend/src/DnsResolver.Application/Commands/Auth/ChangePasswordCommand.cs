namespace DnsResolver.Application.Commands.Auth;

public record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword);

public record ChangePasswordResult(bool Success, string? ErrorMessage);
