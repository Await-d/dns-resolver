namespace DnsResolver.Application.Commands.Auth;

public record LoginCommand(string Username, string Password);

public record LoginResult(
    bool Success,
    string? Token,
    string? Username,
    string? Role,
    DateTime? ExpiresAt,
    string? ErrorMessage);
