namespace DnsResolver.Infrastructure.DnsProviders;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using DnsResolver.Domain.Services;

public class EdgeOneProvider : BaseDnsProvider
{
    private const string Endpoint = "https://teo.tencentcloudapi.com";

    public override string Name => "edgeone";
    public override string DisplayName => "腾讯 EdgeOne";
    public override DnsProviderFieldMeta FieldMeta => new(
        IdLabel: "SecretId",
        SecretLabel: "SecretKey",
        HelpUrl: "https://console.cloud.tencent.com/cam/capi"
    );

    public EdgeOneProvider(HttpClient httpClient) : base(httpClient) { }

    public override async Task<ProviderResult<IReadOnlyList<string>>> GetDomainsAsync(CancellationToken ct = default)
        => await Task.FromResult(ProviderResult<IReadOnlyList<string>>.Fail(ProviderErrorCode.InvalidParameter, "Use TencentCloud provider for EdgeOne"));

    public override async Task<ProviderResult<IReadOnlyList<DnsRecordInfo>>> GetRecordsAsync(string domain, string? subDomain = null, string? recordType = null, CancellationToken ct = default)
        => await Task.FromResult(ProviderResult<IReadOnlyList<DnsRecordInfo>>.Fail(ProviderErrorCode.InvalidParameter, "Use TencentCloud provider for EdgeOne"));

    public override async Task<ProviderResult<DnsRecordInfo>> AddRecordAsync(string domain, string subDomain, string recordType, string value, int ttl = 600, CancellationToken ct = default)
        => await Task.FromResult(ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.InvalidParameter, "Use TencentCloud provider for EdgeOne"));

    public override async Task<ProviderResult<DnsRecordInfo>> UpdateRecordAsync(string domain, string recordId, string value, int? ttl = null, CancellationToken ct = default)
        => await Task.FromResult(ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.InvalidParameter, "Use TencentCloud provider for EdgeOne"));

    public override async Task<ProviderResult> DeleteRecordAsync(string domain, string recordId, CancellationToken ct = default)
        => await Task.FromResult(ProviderResult.Fail(ProviderErrorCode.InvalidParameter, "Use TencentCloud provider for EdgeOne"));
}
