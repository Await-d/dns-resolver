namespace DnsResolver.Infrastructure.Repositories;

using DnsResolver.Domain.Aggregates.IspProvider;
using DnsResolver.Domain.ValueObjects;
using DnsResolver.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

public class InMemoryIspProviderRepository : IIspProviderRepository
{
    private readonly List<IspProvider> _providers;

    public InMemoryIspProviderRepository(IOptions<DnsSettings> settings)
    {
        _providers = settings.Value.Isps.Select(isp => new IspProvider(
            isp.Id,
            isp.Name,
            DnsServer.Create(isp.PrimaryDns),
            string.IsNullOrEmpty(isp.SecondaryDns) ? null : DnsServer.Create(isp.SecondaryDns)
        )).ToList();
    }

    public Task<IReadOnlyList<IspProvider>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<IspProvider>>(_providers);

    public Task<IspProvider?> FindByIdAsync(string id, CancellationToken cancellationToken = default)
        => Task.FromResult(_providers.FirstOrDefault(p => p.Id == id));

    public Task<IspProvider?> FindByDnsServerAsync(string dnsServer, CancellationToken cancellationToken = default)
        => Task.FromResult(_providers.FirstOrDefault(p => p.HasDnsServer(dnsServer)));
}
