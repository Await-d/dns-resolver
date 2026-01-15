namespace DnsResolver.Domain.Aggregates.DnsQuery;

using DnsResolver.Domain.ValueObjects;

public class DnsQuery
{
    public Guid Id { get; private set; }
    public DomainName Domain { get; private set; }
    public RecordType RecordType { get; private set; }
    public DnsServer DnsServer { get; private set; }
    public string? IspName { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public ResolveResult? Result { get; private set; }

    private DnsQuery()
    {
        Domain = null!;
        RecordType = null!;
        DnsServer = null!;
    }

    public static DnsQuery Create(DomainName domain, RecordType recordType, DnsServer dnsServer, string? ispName = null)
    {
        return new DnsQuery
        {
            Id = Guid.NewGuid(),
            Domain = domain,
            RecordType = recordType,
            DnsServer = dnsServer,
            IspName = ispName ?? "自定义DNS",
            CreatedAt = DateTime.UtcNow
        };
    }

    public void SetResult(IReadOnlyList<DnsRecord> records, int queryTimeMs)
    {
        Result = ResolveResult.Succeeded(records, queryTimeMs);
    }

    public void SetError(string errorMessage)
    {
        Result = ResolveResult.Failed(errorMessage);
    }
}
