namespace DnsResolver.Application.Services;

using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;

public interface IDdnsService
{
    Task<DdnsIpResult> GetCurrentPublicIpAsync(CancellationToken ct = default);
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

public class DdnsService : IDdnsService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DdnsService> _logger;

    public DdnsService(IHttpClientFactory httpClientFactory, ILogger<DdnsService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<DdnsIpResult> GetCurrentPublicIpAsync(CancellationToken ct = default)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(10);

        // 尝试多个 IP 查询服务
        var ipServices = new[]
        {
            ("https://api.ipify.org?format=json", "ipify"),
            ("https://api64.ipify.org?format=json", "ipify64"),
            ("http://ip-api.com/json/?fields=query", "ip-api")
        };

        foreach (var (url, serviceName) in ipServices)
        {
            try
            {
                _logger.LogInformation("正在从 {ServiceName} 获取公网 IP: {Url}", serviceName, url);
                var response = await httpClient.GetStringAsync(url, ct);

                var jsonDoc = JsonDocument.Parse(response);
                var ip = jsonDoc.RootElement.TryGetProperty("ip", out var ipProp)
                    ? ipProp.GetString()
                    : jsonDoc.RootElement.TryGetProperty("query", out var queryProp)
                        ? queryProp.GetString()
                        : null;

                if (!string.IsNullOrEmpty(ip))
                {
                    _logger.LogInformation("成功获取公网 IP: {Ip} (来源: {ServiceName})", ip, serviceName);
                    return DdnsIpResult.Ok(ip, serviceName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "从 {ServiceName} 获取 IP 失败", serviceName);
            }
        }

        return DdnsIpResult.Fail("无法从任何服务获取公网 IP");
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
        var ipResult = await GetCurrentPublicIpAsync(ct);
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
