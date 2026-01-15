namespace DnsResolver.Domain.Aggregates.IspProvider;

public interface IIspProviderRepository
{
    Task<IReadOnlyList<IspProvider>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IspProvider?> FindByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IspProvider?> FindByDnsServerAsync(string dnsServer, CancellationToken cancellationToken = default);
}
