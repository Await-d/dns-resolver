namespace DnsResolver.Application.DTOs;

/// <summary>
/// DNS 服务商字段元数据
/// </summary>
public record DnsProviderFieldMetaDto(
    string? IdLabel,
    string? SecretLabel,
    string? ExtParamLabel,
    string? HelpUrl,
    string? HelpText
);

/// <summary>
/// DNS 域名服务商信息
/// </summary>
public record DnsProviderInfoDto(
    string Id,
    string Name,
    string DisplayName,
    DnsProviderFieldMetaDto FieldMeta
);
