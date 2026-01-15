namespace DnsResolver.Application.DTOs;

public record IspProviderDto(
    string Id,
    string Name,
    string PrimaryDns,
    string? SecondaryDns
);
