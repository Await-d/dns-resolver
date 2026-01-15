namespace DnsResolver.Application.Queries.GetDnsProviders;

using DnsResolver.Application.DTOs;
using DnsResolver.Domain.Services;

public class GetDnsProvidersQueryHandler
{
    private readonly IDnsProviderFactory _factory;

    public GetDnsProvidersQueryHandler(IDnsProviderFactory factory)
    {
        _factory = factory;
    }

    public Task<IReadOnlyList<DnsProviderInfoDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var providers = _factory.GetProviderInfos()
            .Select(p => new DnsProviderInfoDto(p.Id, p.Name, p.DisplayName))
            .OrderBy(p => p.DisplayName)
            .ToList();

        return Task.FromResult<IReadOnlyList<DnsProviderInfoDto>>(providers);
    }
}
