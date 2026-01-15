namespace DnsResolver.Infrastructure.DnsProviders;

using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using DnsResolver.Domain.Services;

public class AliesaProvider : BaseDnsProvider
{
    private const string Endpoint = "https://esa.aliyuncs.com/";
    private const string ApiVersion = "2024-09-10";

    public override string Name => "aliesa";
    public override string DisplayName => "阿里云 ESA";
    public override DnsProviderFieldMeta FieldMeta => new(
        IdLabel: "AccessKey ID",
        SecretLabel: "AccessKey Secret",
        HelpUrl: "https://ram.console.aliyun.com/manage/ak"
    );

    public AliesaProvider(HttpClient httpClient) : base(httpClient) { }

    public override async Task<ProviderResult<IReadOnlyList<string>>> GetDomainsAsync(CancellationToken ct = default)
    {
        try
        {
            var result = await RequestAsync<AliDomainsResponse>("ListSites", new(), ct);
            return ProviderResult<IReadOnlyList<string>>.Ok(result?.Sites?.Select(s => s.SiteName).ToList() ?? []);
        }
        catch (Exception ex) { return ProviderResult<IReadOnlyList<string>>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult<IReadOnlyList<DnsRecordInfo>>> GetRecordsAsync(string domain, string? subDomain = null, string? recordType = null, CancellationToken ct = default)
    {
        try
        {
            var @params = new Dictionary<string, string> { ["SiteName"] = domain };
            var result = await RequestAsync<AliRecordsResponse>("ListRecords", @params, ct);
            var records = result?.Records?.Select(r => new DnsRecordInfo(r.RecordId, domain, r.RecordName, $"{r.RecordName}.{domain}", r.Type, r.Data?.Value ?? "", r.Ttl)).ToList() ?? [];
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
            var @params = new Dictionary<string, string> { ["SiteName"] = domain, ["RecordName"] = subDomain, ["Type"] = recordType, ["Data"] = value, ["Ttl"] = ttl.ToString() };
            var result = await RequestAsync<AliRecordResponse>("CreateRecord", @params, ct);
            return ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo(result?.RecordId ?? "", domain, subDomain, GetFullDomain(subDomain, domain), recordType, value, ttl));
        }
        catch (Exception ex) { return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult<DnsRecordInfo>> UpdateRecordAsync(string domain, string recordId, string value, int? ttl = null, CancellationToken ct = default)
    {
        try
        {
            var @params = new Dictionary<string, string> { ["RecordId"] = recordId, ["Data"] = value };
            if (ttl.HasValue) @params["Ttl"] = ttl.Value.ToString();
            await RequestAsync<AliRecordResponse>("UpdateRecord", @params, ct);
            return ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo(recordId, domain, "", domain, "", value, ttl ?? 600));
        }
        catch (Exception ex) { return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult> DeleteRecordAsync(string domain, string recordId, CancellationToken ct = default)
    {
        try { await RequestAsync<AliRecordResponse>("DeleteRecord", new() { ["RecordId"] = recordId }, ct); return ProviderResult.Ok(); }
        catch (Exception ex) { return ProviderResult.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    private async Task<T?> RequestAsync<T>(string action, Dictionary<string, string> @params, CancellationToken ct)
    {
        @params["Action"] = action; @params["Format"] = "JSON"; @params["Version"] = ApiVersion;
        @params["AccessKeyId"] = Config.Id; @params["SignatureMethod"] = "HMAC-SHA1"; @params["SignatureVersion"] = "1.0";
        @params["SignatureNonce"] = Guid.NewGuid().ToString(); @params["Timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        var queryString = string.Join("&", @params.OrderBy(p => p.Key).Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
        using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(Config.Secret + "&"));
        @params["Signature"] = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes($"GET&%2F&{Uri.EscapeDataString(queryString)}")));
        return await HttpClient.GetFromJsonAsync<T>($"{Endpoint}?{string.Join("&", @params.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"))}", JsonOptions, ct);
    }

    private class AliDomainsResponse { public List<AliSite>? Sites { get; set; } }
    private class AliSite { public string SiteName { get; set; } = ""; }
    private class AliRecordsResponse { public List<AliRecord>? Records { get; set; } }
    private class AliRecord { public string RecordId { get; set; } = ""; public string RecordName { get; set; } = ""; public string Type { get; set; } = ""; public int Ttl { get; set; } public AliRecordData? Data { get; set; } }
    private class AliRecordData { public string Value { get; set; } = ""; }
    private class AliRecordResponse { public string? RecordId { get; set; } }
}
