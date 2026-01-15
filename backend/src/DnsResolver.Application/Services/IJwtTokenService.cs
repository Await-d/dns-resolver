namespace DnsResolver.Application.Services;

using DnsResolver.Domain.Aggregates.User;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
    Guid? ValidateToken(string token);
}
