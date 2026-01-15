namespace DnsResolver.Infrastructure.DnsProviders;

using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using DnsResolver.Domain.Services;

public class BaiduCloudProvider : BaseDnsProvider
{
    private const string Endpoint = "https://bcd.baidubce.com";

    public override string Name => "baiducloud";
    public override string DisplayName => "百度云 DNS";
    public override DnsProviderFieldMeta FieldMeta => new(
        IdLabel: "AccessKey ID",
        SecretLabel: "AccessKey Secret",
        HelpUrl: "https://console.bce.baidu.com/iam/#/iam/accesslist"
    );

    public BaiduCloudProvider(HttpClient httpClient) : base(httpClient) { }

    public override async Task<ProviderResult<IReadOnlyList<string>>> GetDomainsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await SendRequestAsync<BdZonesResponse>(HttpMethod.Get, "/v1/domain", null, ct);
            return ProviderResult<IReadOnlyList<string>>.Ok(response?.Zones?.Select(z => z.Name).ToList() ?? []);
        }
        catch (Exception ex) { return ProviderResult<IReadOnlyList<string>>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult<IReadOnlyList<DnsRecordInfo>>> GetRecordsAsync(
        string domain, string? subDomain = null, string? recordType = null, CancellationToken ct = default)
    {
        try
        {
            var response = await SendRequestAsync<BdRecordsResponse>(HttpMethod.Get, $"/v1/domain/{domain}/record", null, ct);
            var records = response?.Result?.Select(r => new DnsRecordInfo(
                r.RecordId.ToString(), domain, r.Rr, GetFullDomain(r.Rr, domain), r.Type, r.Value, r.Ttl
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
            var body = new { rr = subDomain, type = recordType, value, ttl };
            var response = await SendRequestAsync<BdRecordResponse>(HttpMethod.Post, $"/v1/domain/{domain}/record", body, ct);
            return ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo(response?.RecordId.ToString() ?? "", domain, subDomain, GetFullDomain(subDomain, domain), recordType, value, ttl));
        }
        catch (Exception ex) { return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult<DnsRecordInfo>> UpdateRecordAsync(string domain, string recordId, string value, int? ttl = null, CancellationToken ct = default)
    {
        try
        {
            var body = new { value, ttl = ttl ?? 600 };
            await SendRequestAsync<object>(HttpMethod.Put, $"/v1/domain/{domain}/record/{recordId}", body, ct);
            return ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo(recordId, domain, "", domain, "", value, ttl ?? 600));
        }
        catch (Exception ex) { return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult> DeleteRecordAsync(string domain, string recordId, CancellationToken ct = default)
    {
        try
        {
            await SendRequestAsync<object>(HttpMethod.Delete, $"/v1/domain/{domain}/record/{recordId}", null, ct);
            return ProviderResult.Ok();
        }
        catch (Exception ex) { return ProviderResult.Fail(ProviderErrorCode.NetworkError, ex.Message); }
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
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        request.Headers.Add("x-bce-date", timestamp);
        request.Headers.Add("Authorization", $"bce-auth-v1/{Config.Id}/{timestamp}/1800//placeholder");
    }

    private class BdZonesResponse { public List<BdZone>? Zones { get; set; } }
    private class BdZone { public string Name { get; set; } = ""; }
    private class BdRecordsResponse { public List<BdRecord>? Result { get; set; } }
    private class BdRecord { public long RecordId { get; set; } public string Rr { get; set; } = ""; public string Type { get; set; } = ""; public string Value { get; set; } = ""; public int Ttl { get; set; } }
    private class BdRecordResponse { public long RecordId { get; set; } }
}
