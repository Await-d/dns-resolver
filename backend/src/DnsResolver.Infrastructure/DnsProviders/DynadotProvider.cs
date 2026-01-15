namespace DnsResolver.Infrastructure.DnsProviders;

using System.Xml.Linq;
using DnsResolver.Domain.Services;

public class DynadotProvider : BaseDnsProvider
{
    private const string Endpoint = "https://api.dynadot.com/api3.xml";

    public override string Name => "dynadot";
    public override string DisplayName => "Dynadot";

    public DynadotProvider(HttpClient httpClient) : base(httpClient) { }

    public override async Task<ProviderResult<IReadOnlyList<string>>> GetDomainsAsync(CancellationToken ct = default)
    {
        try
        {
            var xml = await HttpClient.GetStringAsync($"{Endpoint}?key={Config.Secret}&command=list_domain", ct);
            var doc = XDocument.Parse(xml);
            var domains = doc.Descendants("Domain").Select(d => d.Element("Name")?.Value ?? "").Where(n => !string.IsNullOrEmpty(n)).ToList();
            return ProviderResult<IReadOnlyList<string>>.Ok(domains);
        }
        catch (Exception ex) { return ProviderResult<IReadOnlyList<string>>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult<IReadOnlyList<DnsRecordInfo>>> GetRecordsAsync(string domain, string? subDomain = null, string? recordType = null, CancellationToken ct = default)
    {
        try
        {
            var xml = await HttpClient.GetStringAsync($"{Endpoint}?key={Config.Secret}&command=get_dns&domain={domain}", ct);
            var doc = XDocument.Parse(xml);
            var records = doc.Descendants("DnsRecord").Select(r => new DnsRecordInfo(
                r.Element("RecordId")?.Value ?? Guid.NewGuid().ToString(), domain, r.Element("Subdomain")?.Value ?? "@",
                GetFullDomain(r.Element("Subdomain")?.Value ?? "@", domain), r.Element("RecordType")?.Value ?? "A",
                r.Element("Value")?.Value ?? "", int.TryParse(r.Element("Ttl")?.Value, out var ttl) ? ttl : 600
            )).ToList();
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
            var url = $"{Endpoint}?key={Config.Secret}&command=set_dns2&domain={domain}&subdomain0={Uri.EscapeDataString(subDomain)}&sub_record_type0={recordType}&sub_record0={Uri.EscapeDataString(value)}&ttl={ttl}";
            var xml = await HttpClient.GetStringAsync(url, ct);
            if (xml.Contains("<Status>error</Status>")) return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.UnknownError, "Failed");
            return ProviderResult<DnsRecordInfo>.Ok(new DnsRecordInfo(Guid.NewGuid().ToString(), domain, subDomain, GetFullDomain(subDomain, domain), recordType, value, ttl));
        }
        catch (Exception ex) { return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }

    public override async Task<ProviderResult<DnsRecordInfo>> UpdateRecordAsync(string domain, string recordId, string value, int? ttl = null, CancellationToken ct = default)
    {
        var getResult = await GetRecordsAsync(domain, ct: ct);
        var existing = getResult.Data?.FirstOrDefault(r => r.RecordId == recordId);
        if (existing == null) return ProviderResult<DnsRecordInfo>.Fail(ProviderErrorCode.RecordNotFound, "Record not found");
        return await AddRecordAsync(domain, existing.SubDomain, existing.RecordType, value, ttl ?? existing.Ttl, ct);
    }

    public override async Task<ProviderResult> DeleteRecordAsync(string domain, string recordId, CancellationToken ct = default)
    {
        try { await HttpClient.GetStringAsync($"{Endpoint}?key={Config.Secret}&command=set_dns2&domain={domain}", ct); return ProviderResult.Ok(); }
        catch (Exception ex) { return ProviderResult.Fail(ProviderErrorCode.NetworkError, ex.Message); }
    }
}
