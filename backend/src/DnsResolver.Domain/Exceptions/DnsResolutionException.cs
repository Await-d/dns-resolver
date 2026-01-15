namespace DnsResolver.Domain.Exceptions;

public class DnsResolutionException : DomainException
{
    public string DnsServer { get; }
    
    public DnsResolutionException(string message, string dnsServer) : base(message)
    {
        DnsServer = dnsServer;
    }
}
