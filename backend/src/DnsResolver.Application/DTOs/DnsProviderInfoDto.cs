namespace DnsResolver.Application.DTOs;

/// <summary>
/// DNS 域名服务商信息
/// </summary>
public record DnsProviderInfoDto(
    string Id,
    string Name,
    string DisplayName
);
