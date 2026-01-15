namespace DnsResolver.Domain.Aggregates.DdnsTask;

/// <summary>
/// DDNS 自动更新任务聚合根
/// </summary>
public class DdnsTask
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string ProviderName { get; private set; }
    public string ProviderId { get; private set; }
    public string ProviderSecret { get; private set; }
    public string Domain { get; private set; }
    public string RecordId { get; private set; }
    public string? SubDomain { get; private set; }
    public int Ttl { get; private set; }
    public int IntervalMinutes { get; private set; }
    public bool Enabled { get; private set; }
    public string? LastKnownIp { get; private set; }
    public DateTime? LastCheckTime { get; private set; }
    public DateTime? LastUpdateTime { get; private set; }
    public string? LastError { get; private set; }
    public Dictionary<string, string>? ExtraParams { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private DdnsTask()
    {
        Name = string.Empty;
        ProviderName = string.Empty;
        ProviderId = string.Empty;
        ProviderSecret = string.Empty;
        Domain = string.Empty;
        RecordId = string.Empty;
    }

    public static DdnsTask Create(
        string name,
        string providerName,
        string providerId,
        string providerSecret,
        string domain,
        string recordId,
        string? subDomain,
        int ttl = 600,
        int intervalMinutes = 5,
        Dictionary<string, string>? extraParams = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Task name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(providerName))
            throw new ArgumentException("Provider name cannot be empty", nameof(providerName));

        if (string.IsNullOrWhiteSpace(domain))
            throw new ArgumentException("Domain cannot be empty", nameof(domain));

        if (string.IsNullOrWhiteSpace(recordId))
            throw new ArgumentException("Record ID cannot be empty", nameof(recordId));

        if (intervalMinutes < 1)
            throw new ArgumentException("Interval must be at least 1 minute", nameof(intervalMinutes));

        var now = DateTime.UtcNow;
        return new DdnsTask
        {
            Id = Guid.NewGuid(),
            Name = name,
            ProviderName = providerName,
            ProviderId = providerId,
            ProviderSecret = providerSecret,
            Domain = domain,
            RecordId = recordId,
            SubDomain = subDomain,
            Ttl = ttl,
            IntervalMinutes = intervalMinutes,
            Enabled = true,
            ExtraParams = extraParams,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void UpdateIp(string newIp)
    {
        LastKnownIp = newIp;
        LastUpdateTime = DateTime.UtcNow;
        LastError = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordCheck()
    {
        LastCheckTime = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordError(string error)
    {
        LastError = error;
        LastCheckTime = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Enable()
    {
        Enabled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable()
    {
        Enabled = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateInterval(int intervalMinutes)
    {
        if (intervalMinutes < 1)
            throw new ArgumentException("Interval must be at least 1 minute", nameof(intervalMinutes));

        IntervalMinutes = intervalMinutes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCredentials(string providerId, string providerSecret)
    {
        ProviderId = providerId;
        ProviderSecret = providerSecret;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool ShouldCheck()
    {
        if (!Enabled) return false;
        if (LastCheckTime == null) return true;

        var nextCheckTime = LastCheckTime.Value.AddMinutes(IntervalMinutes);
        return DateTime.UtcNow >= nextCheckTime;
    }
}
