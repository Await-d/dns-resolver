namespace DnsResolver.Domain.Services;

/// <summary>
/// DNS 服务商信息
/// </summary>
public record DnsProviderInfo(string Id, string Name, string DisplayName);

/// <summary>
/// DNS 服务商工厂接口
/// </summary>
public interface IDnsProviderFactory
{
    IEnumerable<DnsProviderInfo> GetProviderInfos();
}
