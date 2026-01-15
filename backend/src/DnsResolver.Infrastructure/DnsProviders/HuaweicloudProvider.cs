namespace DnsResolver.Infrastructure.DnsProviders;

using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using DnsResolver.Domain.Services;

public class HuaweicloudProvider : BaseDnsProvider
{
    private const string Endpoint = "https://dns.myhuaweicloud.com/v2";

    public override string Name => "huaweicloud";
    public override string DisplayName => "华为云 DNS";
    public override DnsProviderFieldMeta FieldMeta => new(
        IdLabel: "Access Key Id",
        SecretLabel: "Secret Access Key",
        HelpUrl: "https://console.huaweicloud.com/iam/#/mine/accessKey"
    );

    public HuaweicloudProvider(HttpClient httpClient) : base(httpClient) { }

    public override async Task<ProviderResult<IReadOnlyList<string>>> GetDomainsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await SendRequestAsync<HwZonesResponse>(HttpMethod.Get, "/zones", null, ct);
            return ProviderResult<IReadOnlyList<string>>.Ok(response?.Zones?.Select(z => z.Name.TrimEnd('.')).ToList() ?? []);
        }
        catch (Exception ex) { return ProviderResult<IReadOnlyList<string>>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult<IReadOnlyList<DnsRecordInfo>>> GetRecordsAsync(
        string domain, string? subDomain = null, string? recordType = null, CancellationToken ct = default)
    {
        try
        {
            var zoneId = await GetZoneIdAsync(domain, ct);
            if (zoneId == null) return ProviderResult<IReadOnlyList<DnsRecordInfo>>.Fail(ProviderErrorCode.DomainNotFound, "Zone not found");

            var response = await SendRequestAsync<HwRecordsResponse>(HttpMethod.Get, $"/zones/{zoneId}/recordsets", null, ct);
            var records = response?.Recordsets?.Select(r => new DnsRecordInfo(
                r.Id, domain, r.Name.TrimEnd('.') == domain ? "@" : r.Name.TrimEnd('.').Replace($".{domain}", ""),
                r.Name.TrimEnd('.'), r.Type, r.Records?.FirstOrDefault() ?? "", r.Ttl
            )).ToList() ?? [];
            if (!string.IsNullOrEmpty(subDomain)) records = records.Where(r => r.SubDomain == subDomain).ToList();
            if (!string.IsNullOrEmpty(recordType)) records = records.Where(r => r.RecordType == recordType).ToList();
            return ProviderResult<IReadOnlyList<DnsRecordInfo>>.Ok(records);
        }
        catch (Exception ex) { return ProviderResult<IReadOnlyList<DnsRecordInfo>>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult<DnsRecordInfo>> AddRecordAsync(string domain, string subDomain, string recordType, string value, int ttl = 600, CancellationToken ct = default)
    {
        try
        {
            var zoneId = await GetZoneIdAsync(domain, ct);
            if (zoneId == null) return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.DomainNotFound, "Zone not found");

            var fullDomain = GetFullDomain(subDomain, domain) + ".";
            var body = new { name = fullDomain, type = recordType, ttl, records = new[] { value } };
            var response = await SendRequestAsync<HwRecordset>(HttpMethod.Post, $"/zones/{zoneId}/recordsets", body, ct);
            return ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo(response?.Id ?? "", domain, subDomain, fullDomain.TrimEnd('.'), recordType, value, ttl));
        }
        catch (Exception ex) { return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult<DnsRecordInfo>> UpdateRecordAsync(string domain, string recordId, string value, int? ttl = null, CancellationToken ct = default)
    {
        try
        {
            var zoneId = await GetZoneIdAsync(domain, ct);
            if (zoneId == null) return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.DomainNotFound, "Zone not found");

            var body = new { records = new[] { value }, ttl = ttl ?? 600 };
            await SendRequestAsync<HwRecordset>(HttpMethod.Put, $"/zones/{zoneId}/recordsets/{recordId}", body, ct);
            return ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo(recordId, domain, "", domain, "", value, ttl ?? 600));
        }
        catch (Exception ex) { return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult> DeleteRecordAsync(string domain, string recordId, CancellationToken ct = default)
    {
        try
        {
            var zoneId = await GetZoneIdAsync(domain, ct);
            if (zoneId == null) return ProviderResult.Fail(ProviderErrorCode.DomainNotFound, "Zone not found");
            await SendRequestAsync<object>(HttpMethod.Delete, $"/zones/{zoneId}/recordsets/{recordId}", null, ct);
            return ProviderResult.Ok();
        }
        catch (Exception ex) { return ProviderResult.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    private async Task<string?> GetZoneIdAsync(string domain, CancellationToken ct)
    {
        var response = await SendRequestAsync<HwZonesResponse>(HttpMethod.Get, $"/zones?name={domain}.", null, ct);
        return response?.Zones?.FirstOrDefault()?.Id;
    }

    private async Task<T?> SendRequestAsync<T>(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        var request = new HttpRequestMessage(method, $"{Endpoint}{path}");
        if (body != null) request.Content = JsonContent.Create(body, options: JsonOptions);
        SignRequest(request);
        var response = await HttpClient.SendAsync(request, ct);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
    }

    private void SignRequest(HttpRequestMessage request)
    {
        var date = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");
        request.Headers.Add("X-Sdk-Date", date);
        request.Headers.Add("Authorization", $"SDK-HMAC-SHA256 Access={Config.Id}, Signature=placeholder");
    }

    private class HwZonesResponse { public List<HwZone>? Zones { get; set; } }
    private class HwZone { public string Id { get; set; } = ""; public string Name { get; set; } = ""; }
    private class HwRecordsResponse { public List<HwRecordset>? Recordsets { get; set; } }
    private class HwRecordset { public string Id { get; set; } = ""; public string Name { get; set; } = ""; public string Type { get; set; } = ""; public int Ttl { get; set; } public List<string>? Records { get; set; } }
}
