namespace DnsResolver.Infrastructure.DnsProviders;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using DnsResolver.Domain.Services;

public class NSOneProvider : BaseDnsProvider
{
    private const string Endpoint = "https://api.nsone.net/v1";

    public override string Name => "nsone";
    public override string DisplayName => "NS1";

    public NSOneProvider(HttpClient httpClient) : base(httpClient) { }

    public override void Configure(DnsProviderConfig config)
    {
        base.Configure(config);
        HttpClient.DefaultRequestHeaders.Add("X-NSONE-Key", config.Secret);
    }

    public override async Task<ProviderResult<IReadOnlyList<string>>> GetDomainsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await HttpClient.GetFromJsonAsync<List<Ns1Zone>>($"{Endpoint}/zones", JsonOptions, ct);
            return ProviderResult<IReadOnlyList<string>>.Ok(response?.Select(z => z.Zone).ToList() ?? []);
        }
        catch (Exception ex) { return ProviderResult<IReadOnlyList<string>>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult<IReadOnlyList<DnsRecordInfo>>> GetRecordsAsync(string domain, string? subDomain = null, string? recordType = null, CancellationToken ct = default)
    {
        try
        {
            var response = await HttpClient.GetFromJsonAsync<Ns1ZoneDetail>($"{Endpoint}/zones/{domain}", JsonOptions, ct);
            var records = response?.Records?.Select(r => new DnsRecordInfo(
                $"{r.Domain}_{r.Type}", domain, r.Domain == domain ? "@" : r.Domain.Replace($".{domain}", ""),
                r.Domain, r.Type, r.ShortAnswers?.FirstOrDefault() ?? "", r.Ttl
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
            var fullDomain = GetFullDomain(subDomain, domain);
            var body = new { zone = domain, domain = fullDomain, type = recordType, ttl, answers = new[] { new { answer = new[] { value } } } };
            var response = await HttpClient.PutAsJsonAsync($"{Endpoint}/zones/{domain}/{fullDomain}/{recordType}", body, JsonOptions, ct);
            return response.IsSuccessStatusCode ? ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo($"{fullDomain}_{recordType}", domain, subDomain, fullDomain, recordType, value, ttl))
                : ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.UnknownError, await response.Content.ReadAsStringAsync(ct));
        }
        catch (Exception ex) { return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult<DnsRecordInfo>> UpdateRecordAsync(string domain, string recordId, string value, int? ttl = null, CancellationToken ct = default)
    {
        var parts = recordId.Split('_', 2);
        var fullDomain = parts[0];
        var recordType = parts.Length > 1 ? parts[1] : "A";
        var subDomain = fullDomain == domain ? "@" : fullDomain.Replace($".{domain}", "");
        return await AddRecordAsync(domain, subDomain, recordType, value, ttl ?? 600, ct);
    }

    public override async Task<ProviderResult> DeleteRecordAsync(string domain, string recordId, CancellationToken ct = default)
    {
        try
        {
            var parts = recordId.Split('_', 2);
            var response = await HttpClient.DeleteAsync($"{Endpoint}/zones/{domain}/{parts[0]}/{parts[1]}", ct);
            return response.IsSuccessStatusCode ? ProviderResult.Ok() : ProviderResult.Fail(ProviderErrorCode.UnknownError, "Failed");
        }
        catch (Exception ex) { return ProviderResult.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    private class Ns1Zone { public string Zone { get; set; } = ""; }
    private class Ns1ZoneDetail { public List<Ns1Record>? Records { get; set; } }
    private class Ns1Record { public string Domain { get; set; } = ""; public string Type { get; set; } = ""; public int Ttl { get; set; } public List<string>? ShortAnswers { get; set; } }
}
