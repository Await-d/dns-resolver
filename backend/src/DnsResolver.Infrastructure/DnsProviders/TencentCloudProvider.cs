namespace DnsResolver.Infrastructure.DnsProviders;

using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using DnsResolver.Domain.Services;

public class TencentCloudProvider : BaseDnsProvider
{
    private const string Endpoint = "https://dnspod.tencentcloudapi.com";
    private const string Service = "dnspod";
    private const string Version = "2021-03-23";

    public override string Name => "tencentcloud";
    public override string DisplayName => "腾讯云 DNS";

    public TencentCloudProvider(HttpClient httpClient) : base(httpClient) { }

    public override async Task<ProviderResult<IReadOnlyList<string>>> GetDomainsAsync(CancellationToken ct = default)
    {
        try
        {
            var result = await RequestAsync<TcDomainsResponse>("DescribeDomainList", new { }, ct);
            return ProviderResult<IReadOnlyList<string>>.Ok(result?.Response?.DomainList?.Select(d => d.Name).ToList() ?? []);
        }
        catch (Exception ex) { return ProviderResult<IReadOnlyList<string>>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult<IReadOnlyList<DnsRecordInfo>>> GetRecordsAsync(string domain, string? subDomain = null, string? recordType = null, CancellationToken ct = default)
    {
        try
        {
            var body = new { Domain = domain, Subdomain = subDomain ?? "", RecordType = recordType ?? "" };
            var result = await RequestAsync<TcRecordsResponse>("DescribeRecordList", body, ct);
            var records = result?.Response?.RecordList?.Select(r => new DnsRecordInfo(r.RecordId.ToString(), domain, r.Name, $"{r.Name}.{domain}", r.Type, r.Value, r.TTL)).ToList() ?? [];
            return ProviderResult<IReadOnlyList<DnsRecordInfo>>.Ok(records);
        }
        catch (Exception ex) { return ProviderResult<IReadOnlyList<DnsRecordInfo>>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult<DnsRecordInfo>> AddRecordAsync(string domain, string subDomain, string recordType, string value, int ttl = 600, CancellationToken ct = default)
    {
        try
        {
            var body = new { Domain = domain, SubDomain = subDomain, RecordType = recordType, RecordLine = "默认", Value = value, TTL = ttl };
            var result = await RequestAsync<TcRecordResponse>("CreateRecord", body, ct);
            return ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo(result?.Response?.RecordId?.ToString() ?? "", domain, subDomain, GetFullDomain(subDomain, domain), recordType, value, ttl));
        }
        catch (Exception ex) { return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult<DnsRecordInfo>> UpdateRecordAsync(string domain, string recordId, string value, int? ttl = null, CancellationToken ct = default)
    {
        try
        {
            var getResult = await GetRecordsAsync(domain, ct: ct);
            var existing = getResult.Data?.FirstOrDefault(r => r.RecordId == recordId);
            if (existing == null) return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.RecordNotFound, "Record not found");

            var body = new { Domain = domain, RecordId = long.Parse(recordId), SubDomain = existing.SubDomain, RecordType = existing.RecordType, RecordLine = "默认", Value = value, TTL = ttl ?? existing.Ttl };
            await RequestAsync<TcRecordResponse>("ModifyRecord", body, ct);
            return ProviderResult<DnsRecordInfo>.Ok(existing with { Value = value, Ttl = ttl ?? existing.Ttl });
        }
        catch (Exception ex) { return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult> DeleteRecordAsync(string domain, string recordId, CancellationToken ct = default)
    {
        try
        {
            await RequestAsync<TcRecordResponse>("DeleteRecord", new { Domain = domain, RecordId = long.Parse(recordId) }, ct);
            return ProviderResult.Ok();
        }
        catch (Exception ex) { return ProviderResult.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    private async Task<T?> RequestAsync<T>(string action, object data, CancellationToken ct)
    {
        var payload = System.Text.Json.JsonSerializer.Serialize(data, JsonOptions);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var date = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime.ToString("yyyy-MM-dd");

        var hashedPayload = Sha256Hex(payload);
        var canonicalRequest = $"POST\n/\n\ncontent-type:application/json\nhost:{Service}.tencentcloudapi.com\nx-tc-action:{action.ToLower()}\n\ncontent-type;host;x-tc-action\n{hashedPayload}";
        var stringToSign = $"TC3-HMAC-SHA256\n{timestamp}\n{date}/{Service}/tc3_request\n{Sha256Hex(canonicalRequest)}";

        var secretDate = HmacSha256($"TC3{Config.Secret}", date);
        var secretService = HmacSha256(secretDate, Service);
        var secretSigning = HmacSha256(secretService, "tc3_request");
        var signature = Convert.ToHexString(HmacSha256(secretSigning, stringToSign)).ToLower();

        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        request.Headers.Add("Authorization", $"TC3-HMAC-SHA256 Credential={Config.Id}/{date}/{Service}/tc3_request, SignedHeaders=content-type;host;x-tc-action, Signature={signature}");
        request.Headers.Add("Host", $"{Service}.tencentcloudapi.com");
        request.Headers.Add("X-TC-Action", action);
        request.Headers.Add("X-TC-Timestamp", timestamp.ToString());
        request.Headers.Add("X-TC-Version", Version);

        var response = await HttpClient.SendAsync(request, ct);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
    }

    private static string Sha256Hex(string input) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input))).ToLower();
    private static byte[] HmacSha256(string key, string data) => HMACSHA256.HashData(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(data));
    private static byte[] HmacSha256(byte[] key, string data) => HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(data));

    private class TcDomainsResponse { public TcDomainsData? Response { get; set; } }
    private class TcDomainsData { public List<TcDomain>? DomainList { get; set; } }
    private class TcDomain { public string Name { get; set; } = ""; }
    private class TcRecordsResponse { public TcRecordsData? Response { get; set; } }
    private class TcRecordsData { public List<TcRecord>? RecordList { get; set; } }
    private class TcRecord { public long RecordId { get; set; } public string Name { get; set; } = ""; public string Type { get; set; } = ""; public string Value { get; set; } = ""; public int TTL { get; set; } }
    private class TcRecordResponse { public TcRecordData? Response { get; set; } }
    private class TcRecordData { public long? RecordId { get; set; } }
}
