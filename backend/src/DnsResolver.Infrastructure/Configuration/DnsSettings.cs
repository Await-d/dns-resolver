namespace DnsResolver.Infrastructure.Configuration;

public class DnsSettings
{
    public int QueryTimeoutSeconds { get; set; } = 5;
    public int Retries { get; set; } = 2;
    public List<IspConfig> Isps { get; set; } = new();
}

public class IspConfig
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PrimaryDns { get; set; } = string.Empty;
    public string? SecondaryDns { get; set; }
}
