namespace DnsResolver.Domain.Services;

using DnsResolver.Domain.Aggregates.DnsQuery;
using DnsResolver.Domain.Aggregates.IspProvider;
using DnsResolver.Domain.ValueObjects;

public interface IDnsResolutionService
{
    Task<DnsQuery> ResolveAsync(
        DomainName domain,
        RecordType recordType,
        DnsServer dnsServer,
        string? ispName = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DnsQuery>> BatchResolveAsync(
        DomainName domain,
        RecordType recordType,
        IEnumerable<IspProvider> providers,
        CancellationToken cancellationToken = default);
}
