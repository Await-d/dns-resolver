namespace DnsResolver.Infrastructure.DnsClient;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using global::DnsClient;
using global::DnsClient.Protocol;
using DnsResolver.Domain.Aggregates.DnsQuery;
using DnsResolver.Domain.Aggregates.IspProvider;
using DnsResolver.Domain.Services;
using DnsResolver.Domain.ValueObjects;
using DnsResolver.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class DnsClientAdapter : IDnsResolutionService
{
    private readonly ConcurrentDictionary<string, LookupClient> _clientCache = new();
    private readonly DnsSettings _settings;
    private readonly ILogger<DnsClientAdapter> _logger;

    public DnsClientAdapter(IOptions<DnsSettings> settings, ILogger<DnsClientAdapter> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<DnsQuery> ResolveAsync(
        DomainName domain,
        RecordType recordType,
        DnsServer dnsServer,
        string? ispName = null,
        CancellationToken cancellationToken = default)
    {
        var query = DnsQuery.Create(domain, recordType, dnsServer, ispName);
        var client = GetOrCreateClient(dnsServer);

        try
        {
            var queryType = MapRecordType(recordType);
            var stopwatch = Stopwatch.StartNew();

            var result = await client.QueryAsync(
                domain.Value, queryType, cancellationToken: cancellationToken);

            stopwatch.Stop();

            var records = ExtractRecords(result, recordType);
            query.SetResult(records, (int)stopwatch.ElapsedMilliseconds);

            _logger.LogInformation(
                "DNS 解析成功: {Domain} -> {Count} 条记录, 耗时 {Ms}ms",
                domain, records.Count, stopwatch.ElapsedMilliseconds);
        }
        catch (DnsResponseException ex)
        {
            _logger.LogWarning(ex, "DNS 响应错误: {Domain} via {DnsServer}", domain, dnsServer);
            query.SetError($"DNS 响应错误: {ex.Code}");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DNS 查询超时: {Domain} via {DnsServer}", domain, dnsServer);
            query.SetError("查询超时");
        }
        catch (SocketException ex)
        {
            _logger.LogWarning(ex, "网络错误: {Domain} via {DnsServer}", domain, dnsServer);
            query.SetError($"网络错误: {ex.SocketErrorCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DNS 解析失败: {Domain} via {DnsServer}", domain, dnsServer);
            query.SetError(ex.Message);
        }

        return query;
    }

    public async Task<IReadOnlyList<DnsQuery>> BatchResolveAsync(
        DomainName domain,
        RecordType recordType,
        IEnumerable<IspProvider> providers,
        CancellationToken cancellationToken = default)
    {
        var tasks = providers.Select(p =>
            ResolveAsync(domain, recordType, p.PrimaryDns, p.Name, cancellationToken));

        var results = await Task.WhenAll(tasks);
        return results;
    }

    private LookupClient GetOrCreateClient(DnsServer dnsServer)
    {
        return _clientCache.GetOrAdd(dnsServer.Address, _ =>
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(dnsServer.Address), dnsServer.Port);
            var options = new LookupClientOptions(endpoint)
            {
                Timeout = TimeSpan.FromSeconds(_settings.QueryTimeoutSeconds),
                Retries = _settings.Retries
            };
            return new LookupClient(options);
        });
    }

    private static QueryType MapRecordType(RecordType recordType) => recordType.Value switch
    {
        "A" => QueryType.A,
        "AAAA" => QueryType.AAAA,
        "CNAME" => QueryType.CNAME,
        "MX" => QueryType.MX,
        "TXT" => QueryType.TXT,
        "NS" => QueryType.NS,
        "SOA" => QueryType.SOA,
        _ => QueryType.A
    };

    private static List<DnsRecord> ExtractRecords(IDnsQueryResponse response, RecordType recordType)
    {
        var records = new List<DnsRecord>();

        foreach (var answer in response.Answers)
        {
            var value = answer switch
            {
                ARecord a => a.Address.ToString(),
                AaaaRecord aaaa => aaaa.Address.ToString(),
                CNameRecord cname => cname.CanonicalName.Value,
                MxRecord mx => $"{mx.Preference} {mx.Exchange.Value}",
                TxtRecord txt => string.Join(" ", txt.Text),
                NsRecord ns => ns.NSDName.Value,
                SoaRecord soa => $"{soa.MName.Value} {soa.RName.Value}",
                _ => answer.ToString() ?? ""
            };

            records.Add(new DnsRecord(value, answer.TimeToLive, recordType));
        }

        return records;
    }
}
