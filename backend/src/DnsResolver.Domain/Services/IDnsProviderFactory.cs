namespace DnsResolver.Domain.Services;

/// <summary>
/// DNS 服务商字段元数据
/// </summary>
public record DnsProviderFieldMeta(
    string? IdLabel,
    string? SecretLabel,
    string? ExtParamLabel = null,
    string? HelpUrl = null,
    string? HelpText = null
);

/// <summary>
/// DNS 服务商信息
/// </summary>
public record DnsProviderInfo(
    string Id,
    string Name,
    string DisplayName,
    DnsProviderFieldMeta FieldMeta
);

/// <summary>
/// DNS 服务商工厂接口
/// </summary>
public interface IDnsProviderFactory
{
    IEnumerable<DnsProviderInfo> GetProviderInfos();
}
