namespace DnsResolver.Infrastructure.DnsProviders;

using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using DnsResolver.Domain.Services;

public class AlidnsProvider : BaseDnsProvider
{
    private const string Endpoint = "https://alidns.aliyuncs.com/";
    private const string ApiVersion = "2015-01-09";

    public override string Name => "alidns";
    public override string DisplayName => "阿里云 DNS";

    public AlidnsProvider(HttpClient httpClient) : base(httpClient) { }

    public override async Task<ProviderResult<IReadOnlyList<string>>> GetDomainsAsync(CancellationToken ct = default)
    {
        try
        {
            var result = await RequestAsync<AliDomainsResponse>("DescribeDomains", new(), ct);
            return ProviderResult<IReadOnlyList<string>>.Ok(
                result?.Domains?.Domain?.Select(d => d.DomainName).ToList() ?? []);
        }
        catch (Exception ex)
        {
            return ProviderResult<IReadOnlyList<string>>.Fail(ProviderErrorCode.NetworkError, ex.Message);
        }
    }

    public override async Task<ProviderResult<IReadOnlyList<DnsRecordInfo>>> GetRecordsAsync(
        string domain, string? subDomain = null, string? recordType = null, CancellationToken ct = default)
    {
        try
        {
            var fullDomain = string.IsNullOrEmpty(subDomain) || subDomain == "@" ? domain : $"{subDomain}.{domain}";
            var @params = new Dictionary<string, string> { ["DomainName"] = domain, ["SubDomain"] = fullDomain };
            if (!string.IsNullOrEmpty(recordType)) @params["Type"] = recordType;

            var result = await RequestAsync<AliRecordsResponse>("DescribeSubDomainRecords", @params, ct);
            var records = result?.DomainRecords?.Record?.Select(r => new DnsRecordInfo(
                r.RecordId, domain, r.RR, $"{r.RR}.{domain}", r.Type, r.Value, int.TryParse(r.TTL, out var ttl) ? ttl : 600
            )).ToList() ?? [];

            return ProviderResult<IReadOnlyList<DnsRecordInfo>>.Ok(records);
        }
        catch (Exception ex)
        {
            return ProviderResult<IReadOnlyList<DnsRecordInfo>>.Fail(ProviderErrorCode.NetworkError, ex.Message);
        }
    }

    public override async Task<ProviderResult<DnsRecordInfo>> AddRecordAsync(
        string domain, string subDomain, string recordType, string value, int ttl = 600, CancellationToken ct = default)
    {
        try
        {
            var @params = new Dictionary<string, string>
            {
                ["DomainName"] = domain, ["RR"] = subDomain == "@" ? "@" : subDomain,
                ["Type"] = recordType, ["Value"] = value, ["TTL"] = ttl.ToString()
            };
            var result = await RequestAsync<AliRecordResponse>("AddDomainRecord", @params, ct);
            if (string.IsNullOrEmpty(result?.RecordId))
                return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.UnknownError, "Failed to add record");

            return ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo(
                result.RecordId, domain, subDomain, GetFullDomain(subDomain, domain), recordType, value, ttl));
        }
        catch (Exception ex)
        {
            return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message);
        }
    }

    public override async Task<ProviderResult<DnsRecordInfo>> UpdateRecordAsync(
        string domain, string recordId, string value, int? ttl = null, CancellationToken ct = default)
    {
        try
        {
            var getResult = await GetRecordsAsync(domain, ct: ct);
            var existing = getResult.Data?.FirstOrDefault(r => r.RecordId == recordId);
            if (existing == null)
                return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.RecordNotFound, "Record not found");

            var @params = new Dictionary<string, string>
            {
                ["RecordId"] = recordId, ["RR"] = existing.SubDomain,
                ["Type"] = existing.RecordType, ["Value"] = value, ["TTL"] = (ttl ?? existing.Ttl).ToString()
            };
            var result = await RequestAsync<AliRecordResponse>("UpdateDomainRecord", @params, ct);
            if (string.IsNullOrEmpty(result?.RecordId))
                return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.UnknownError, "Failed to update");

            return ProviderResult<DnsRecordInfo>.Ok(existing with { Value = value, Ttl = ttl ?? existing.Ttl });
        }
        catch (Exception ex)
        {
            return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message);
        }
    }

    public override async Task<ProviderResult> DeleteRecordAsync(string domain, string recordId, CancellationToken ct = default)
    {
        try
        {
            await RequestAsync<AliRecordResponse>("DeleteDomainRecord", new() { ["RecordId"] = recordId }, ct);
            return ProviderResult.Ok();
        }
        catch (Exception ex)
        {
            return ProviderResult.Fail(ProviderErrorCode.NetworkError, ex.Message);
        }
    }

    private async Task<T?> RequestAsync<T>(string action, Dictionary<string, string> @params, CancellationToken ct)
    {
        @params["Action"] = action;
        @params["Format"] = "JSON";
        @params["Version"] = ApiVersion;
        @params["AccessKeyId"] = Config.Id;
        @params["SignatureMethod"] = "HMAC-SHA1";
        @params["SignatureVersion"] = "1.0";
        @params["SignatureNonce"] = Guid.NewGuid().ToString();
        @params["Timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

        var sortedParams = @params.OrderBy(p => p.Key).ToList();
        var queryString = string.Join("&", sortedParams.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
        var stringToSign = $"GET&%2F&{Uri.EscapeDataString(queryString)}";

        using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(Config.Secret + "&"));
        var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
        @params["Signature"] = signature;

        var url = $"{Endpoint}?{string.Join("&", @params.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"))}";
        return await HttpClient.GetFromJsonAsync<T>(url, JsonOptions, ct);
    }

    private class AliDomainsResponse { public AliDomainList? Domains { get; set; } }
    private class AliDomainList { public List<AliDomain>? Domain { get; set; } }
    private class AliDomain { public string DomainName { get; set; } = ""; }
    private class AliRecordsResponse { public AliRecordList? DomainRecords { get; set; } }
    private class AliRecordList { public List<AliRecord>? Record { get; set; } }
    private class AliRecord { public string RecordId { get; set; } = ""; public string RR { get; set; } = ""; public string Type { get; set; } = ""; public string Value { get; set; } = ""; public string TTL { get; set; } = "600"; }
    private class AliRecordResponse { public string? RecordId { get; set; } }
}
