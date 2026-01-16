namespace DnsResolver.Application.Services;

using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;

public interface IDdnsService
{
    Task<DdnsIpResult> GetCurrentPublicIpAsync(string? preferredSource = null, CancellationToken ct = default);
    IReadOnlyList<IpSourceInfo> GetAvailableIpSources();
    Task<DdnsUpdateResult> CheckAndUpdateDnsAsync(
        string providerName,
        string providerId,
        string providerSecret,
        string domain,
        string recordId,
        string lastKnownIp,
        Dictionary<string, string>? extraParams = null,
        CancellationToken ct = default);
}

public record IpSourceInfo(string Id, string Name, string Url, bool SupportsIpv6);

public class DdnsService : IDdnsService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DdnsService> _logger;

    private static readonly List<IpSourceInfo> IpSources = new()
    {
        new("ipify", "ipify (IPv4)", "https://api.ipify.org?format=json", false),
        new("ipify64", "ipify (IPv6)", "https://api64.ipify.org?format=json", true),
        new("ip-api", "ip-api.com", "http://ip-api.com/json/?fields=query", false),
        new("icanhazip", "icanhazip.com", "https://icanhazip.com", false),
        new("ifconfig", "ifconfig.me", "https://ifconfig.me/ip", false),
        new("ipinfo", "ipinfo.io", "https://ipinfo.io/ip", false),
        new("myip", "api.myip.com", "https://api.myip.com", false),
    };

    public DdnsService(IHttpClientFactory httpClientFactory, ILogger<DdnsService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public IReadOnlyList<IpSourceInfo> GetAvailableIpSources() => IpSources;

    public async Task<DdnsIpResult> GetCurrentPublicIpAsync(string? preferredSource = null, CancellationToken ct = default)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(10);

        // 如果指定了首选来源，优先使用
        var sources = IpSources.ToList();
        if (!string.IsNullOrEmpty(preferredSource))
        {
            var preferred = sources.FirstOrDefault(s => s.Id == preferredSource);
            if (preferred != null)
            {
                sources.Remove(preferred);
                sources.Insert(0, preferred);
            }
        }

        foreach (var source in sources)
        {
            try
            {
                _logger.LogInformation("正在从 {ServiceName} 获取公网 IP: {Url}", source.Name, source.Url);
                var response = await httpClient.GetStringAsync(source.Url, ct);

                var ip = ParseIpFromResponse(response, source.Id);

                if (!string.IsNullOrEmpty(ip))
                {
                    _logger.LogInformation("成功获取公网 IP: {Ip} (来源: {ServiceName})", ip, source.Name);
                    return DdnsIpResult.Ok(ip, source.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "从 {ServiceName} 获取 IP 失败", source.Name);
            }
        }

        return DdnsIpResult.Fail("无法从任何服务获取公网 IP");
    }

    private static string? ParseIpFromResponse(string response, string sourceId)
    {
        response = response.Trim();

        // 尝试解析 JSON 格式
        if (response.StartsWith("{"))
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(response);
                // ipify 格式: {"ip": "x.x.x.x"}
                if (jsonDoc.RootElement.TryGetProperty("ip", out var ipProp))
                    return ipProp.GetString();
                // ip-api 格式: {"query": "x.x.x.x"}
                if (jsonDoc.RootElement.TryGetProperty("query", out var queryProp))
                    return queryProp.GetString();
            }
            catch
            {
                // 忽略 JSON 解析错误
            }
        }

        // 纯文本格式 (icanhazip, ifconfig, ipinfo)
        if (IsValidIp(response))
        {
            return response;
        }

        return null;
    }

    private static bool IsValidIp(string ip)
    {
        return System.Net.IPAddress.TryParse(ip, out _);
    }

    public async Task<DdnsUpdateResult> CheckAndUpdateDnsAsync(
        string providerName,
        string providerId,
        string providerSecret,
        string domain,
        string recordId,
        string lastKnownIp,
        Dictionary<string, string>? extraParams = null,
        CancellationToken ct = default)
    {
        // 获取当前公网 IP
        var ipResult = await GetCurrentPublicIpAsync(null, ct);
        if (!ipResult.Success || string.IsNullOrEmpty(ipResult.Ip))
        {
            return DdnsUpdateResult.Fail(ipResult.ErrorMessage ?? "获取公网 IP 失败", null, null);
        }

        var currentIp = ipResult.Ip;

        // 检查 IP 是否变化
        if (currentIp == lastKnownIp)
        {
            _logger.LogInformation("IP 未变化，无需更新: {Ip}", currentIp);
            return DdnsUpdateResult.NoChange(currentIp, lastKnownIp);
        }

        _logger.LogInformation("检测到 IP 变化: {OldIp} -> {NewIp}", lastKnownIp, currentIp);

        // 注意: 实际的 DNS 更新需要通过 IDnsProvider 接口完成
        // 这里返回需要更新的信息，由调用者完成实际更新
        return DdnsUpdateResult.NeedUpdate(currentIp, lastKnownIp);
    }
}

public record DdnsIpResult(bool Success, string? Ip, string? Source, string? ErrorMessage = null)
{
    public static DdnsIpResult Ok(string ip, string source) => new(true, ip, source);
    public static DdnsIpResult Fail(string error) => new(false, null, null, error);
}

public record DdnsUpdateResult(
    bool Success,
    bool IpChanged,
    string? CurrentIp,
    string? PreviousIp,
    string? ErrorMessage = null)
{
    public static DdnsUpdateResult NoChange(string currentIp, string previousIp)
        => new(true, false, currentIp, previousIp);

    public static DdnsUpdateResult NeedUpdate(string currentIp, string previousIp)
        => new(true, true, currentIp, previousIp);

    public static DdnsUpdateResult Updated(string currentIp, string previousIp)
        => new(true, true, currentIp, previousIp);

    public static DdnsUpdateResult Fail(string error, string? currentIp, string? previousIp)
        => new(false, false, currentIp, previousIp, error);
}
