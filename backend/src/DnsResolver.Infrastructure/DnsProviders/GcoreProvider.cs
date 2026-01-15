namespace DnsResolver.Infrastructure.DnsProviders;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using DnsResolver.Domain.Services;

public class GcoreProvider : BaseDnsProvider
{
    private const string Endpoint = "https://api.gcore.com/dns/v2";

    public override string Name => "gcore";
    public override string DisplayName => "Gcore";

    public GcoreProvider(HttpClient httpClient) : base(httpClient) { }

    public override void Configure(DnsProviderConfig config)
    {
        base.Configure(config);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("APIKey", config.Secret);
    }

    public override async Task<ProviderResult<IReadOnlyList<string>>> GetDomainsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await HttpClient.GetFromJsonAsync<GcZonesResponse>($"{Endpoint}/zones", JsonOptions, ct);
            return ProviderResult<IReadOnlyList<string>>.Ok(response?.Zones?.Select(z => z.Name).ToList() ?? []);
        }
        catch (Exception ex) { return ProviderResult<IReadOnlyList<string>>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult<IReadOnlyList<DnsRecordInfo>>> GetRecordsAsync(string domain, string? subDomain = null, string? recordType = null, CancellationToken ct = default)
    {
        try
        {
            var response = await HttpClient.GetFromJsonAsync<GcRRSetsResponse>($"{Endpoint}/zones/{domain}/rrsets", JsonOptions, ct);
            var records = response?.RRSets?.SelectMany(r => r.Records.Select(rec => new DnsRecordInfo(
                $"{r.Name}_{r.Type}", domain, r.Name == domain ? "@" : r.Name.Replace($".{domain}", "").TrimEnd('.'),
                r.Name, r.Type, rec.Content?.FirstOrDefault() ?? "", r.Ttl
            ))).ToList() ?? [];
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
            var fullDomain = GetFullDomain(subDomain, domain);
            var body = new { resource_records = new[] { new { content = new[] { value } } }, ttl };
            var response = await HttpClient.PostAsJsonAsync($"{Endpoint}/zones/{domain}/{fullDomain}/{recordType}", body, JsonOptions, ct);
            if (!response.IsSuccessStatusCode) return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.UnknownError, await response.Content.ReadAsStringAsync(ct));
            return ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo($"{fullDomain}_{recordType}", domain, subDomain, fullDomain, recordType, value, ttl));
        }
        catch (Exception ex) { return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult<DnsRecordInfo>> UpdateRecordAsync(string domain, string recordId, string value, int? ttl = null, CancellationToken ct = default)
    {
        try
        {
            var parts = recordId.Split('_', 2);
            if (parts.Length != 2) return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.InvalidParameter, "Invalid record ID");
            var body = new { resource_records = new[] { new { content = new[] { value } } }, ttl = ttl ?? 600 };
            var response = await HttpClient.PutAsJsonAsync($"{Endpoint}/zones/{domain}/{parts[0]}/{parts[1]}", body, JsonOptions, ct);
            if (!response.IsSuccessStatusCode) return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.UnknownError, await response.Content.ReadAsStringAsync(ct));
            return ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo(recordId, domain, parts[0] == domain ? "@" : parts[0].Replace($".{domain}", ""), parts[0], parts[1], value, ttl ?? 600));
        }
        catch (Exception ex) { return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult> DeleteRecordAsync(string domain, string recordId, CancellationToken ct = default)
    {
        try
        {
            var parts = recordId.Split('_', 2);
            if (parts.Length != 2) return ProviderResult.Fail(ProviderErrorCode.InvalidParameter, "Invalid record ID");
            var response = await HttpClient.DeleteAsync($"{Endpoint}/zones/{domain}/{parts[0]}/{parts[1]}", ct);
            return response.IsSuccessStatusCode ? ProviderResult.Ok() : ProviderResult.Fail(ProviderErrorCode.UnknownError, "Failed");
        }
        catch (Exception ex) { return ProviderResult.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    private class GcZonesResponse { public List<GcZone>? Zones { get; set; } }
    private class GcZone { public string Name { get; set; } = ""; }
    private class GcRRSetsResponse { public List<GcRRSet>? RRSets { get; set; } }
    private class GcRRSet { public string Name { get; set; } = ""; public string Type { get; set; } = ""; public int Ttl { get; set; } public List<GcRecord> Records { get; set; } = []; }
    private class GcRecord { public List<string>? Content { get; set; } }
}
