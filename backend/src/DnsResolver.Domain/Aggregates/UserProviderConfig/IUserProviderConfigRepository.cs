namespace DnsResolver.Domain.Aggregates.UserProviderConfig;

public interface IUserProviderConfigRepository
{
    Task<IReadOnlyList<UserProviderConfig>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserProviderConfig?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserProviderConfig?> GetByUserAndProviderAsync(Guid userId, string providerName, CancellationToken cancellationToken = default);
    Task AddAsync(UserProviderConfig config, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserProviderConfig config, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
