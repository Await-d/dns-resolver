namespace DnsResolver.Domain.Aggregates.IspProvider;

using DnsResolver.Domain.ValueObjects;

public class IspProvider
{
    public string Id { get; private set; }
    public string Name { get; private set; }
    public DnsServer PrimaryDns { get; private set; }
    public DnsServer? SecondaryDns { get; private set; }

    private IspProvider() 
    {
        Id = string.Empty;
        Name = string.Empty;
        PrimaryDns = null!;
    }

    public IspProvider(string id, string name, DnsServer primaryDns, DnsServer? secondaryDns = null)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        PrimaryDns = primaryDns ?? throw new ArgumentNullException(nameof(primaryDns));
        SecondaryDns = secondaryDns;
    }

    public bool HasDnsServer(string address) =>
        PrimaryDns.Address == address || SecondaryDns?.Address == address;

    public static IspProvider Create(string id, string name, string primaryDns, string? secondaryDns = null) =>
        new(id, name, DnsServer.Create(primaryDns), secondaryDns != null ? DnsServer.Create(secondaryDns) : null);
}
