namespace DnsResolver.Domain.Aggregates.UserProviderConfig;

/// <summary>
/// 用户的 DNS 服务商配置
/// </summary>
public class UserProviderConfig
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string ProviderName { get; private set; }
    public string DisplayName { get; private set; }
    public string ApiId { get; private set; }
    public string ApiSecret { get; private set; }
    public Dictionary<string, string>? ExtraParams { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastUsedAt { get; private set; }

    private UserProviderConfig()
    {
        ProviderName = string.Empty;
        DisplayName = string.Empty;
        ApiId = string.Empty;
        ApiSecret = string.Empty;
    }

    public UserProviderConfig(
        Guid id,
        Guid userId,
        string providerName,
        string displayName,
        string apiId,
        string apiSecret,
        Dictionary<string, string>? extraParams = null)
    {
        Id = id;
        UserId = userId;
        ProviderName = providerName ?? throw new ArgumentNullException(nameof(providerName));
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        ApiId = apiId ?? throw new ArgumentNullException(nameof(apiId));
        ApiSecret = apiSecret ?? throw new ArgumentNullException(nameof(apiSecret));
        ExtraParams = extraParams;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static UserProviderConfig Create(
        Guid userId,
        string providerName,
        string displayName,
        string apiId,
        string apiSecret,
        Dictionary<string, string>? extraParams = null)
    {
        return new UserProviderConfig(
            Guid.NewGuid(),
            userId,
            providerName,
            displayName,
            apiId,
            apiSecret,
            extraParams);
    }

    public void Update(string displayName, string apiId, string apiSecret, Dictionary<string, string>? extraParams)
    {
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        ApiId = apiId ?? throw new ArgumentNullException(nameof(apiId));
        ApiSecret = apiSecret ?? throw new ArgumentNullException(nameof(apiSecret));
        ExtraParams = extraParams;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    public void RecordUsage()
    {
        LastUsedAt = DateTime.UtcNow;
    }
}
