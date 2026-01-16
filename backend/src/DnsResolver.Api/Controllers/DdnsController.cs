namespace DnsResolver.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DnsResolver.Api.Responses;
using DnsResolver.Application.Services;
using DnsResolver.Domain.Services;
using DnsResolver.Infrastructure.DnsProviders;

[ApiController]
[Route("api/v1/ddns")]
[Authorize]
public class DdnsController : ControllerBase
{
    private readonly IDdnsService _ddnsService;
    private readonly DnsProviderFactory _providerFactory;
    private readonly ILogger<DdnsController> _logger;

    public DdnsController(
        IDdnsService ddnsService,
        DnsProviderFactory providerFactory,
        ILogger<DdnsController> logger)
    {
        _ddnsService = ddnsService;
        _providerFactory = providerFactory;
        _logger = logger;
    }

    /// <summary>
    /// 获取可用的 IP 来源列表
    /// </summary>
    [HttpGet("ip-sources")]
    public ActionResult<ApiResponse<IReadOnlyList<IpSourceResponse>>> GetIpSources()
    {
        var sources = _ddnsService.GetAvailableIpSources()
            .Select(s => new IpSourceResponse(s.Id, s.Name, s.SupportsIpv6))
            .ToList();
        return Ok(ApiResponse<IReadOnlyList<IpSourceResponse>>.Ok(sources));
    }

    /// <summary>
    /// 获取当前公网 IP
    /// </summary>
    [HttpGet("ip")]
    public async Task<ActionResult<ApiResponse<DdnsIpResponse>>> GetCurrentIp(
        [FromQuery] string? source,
        CancellationToken ct)
    {
        var result = await _ddnsService.GetCurrentPublicIpAsync(source, ct);

        if (!result.Success)
            return BadRequest(ApiResponse<DdnsIpResponse>.Fail(result.ErrorMessage ?? "获取 IP 失败"));

        var response = new DdnsIpResponse(result.Ip!, result.Source!);
        return Ok(ApiResponse<DdnsIpResponse>.Ok(response));
    }

    /// <summary>
    /// 手动触发 DDNS 更新
    /// </summary>
    [HttpPost("update")]
    public async Task<ActionResult<ApiResponse<DdnsUpdateResponse>>> UpdateDns(
        [FromBody] DdnsUpdateRequest request,
        CancellationToken ct)
    {
        // 获取当前公网 IP
        var ipResult = await _ddnsService.GetCurrentPublicIpAsync(null, ct);
        if (!ipResult.Success || string.IsNullOrEmpty(ipResult.Ip))
        {
            return BadRequest(ApiResponse<DdnsUpdateResponse>.Fail(
                ipResult.ErrorMessage ?? "获取公网 IP 失败"));
        }

        var currentIp = ipResult.Ip;

        // 检查 IP 是否变化
        if (currentIp == request.LastKnownIp && !request.ForceUpdate)
        {
            _logger.LogInformation("IP 未变化，无需更新: {Ip}", currentIp);
            var noChangeResponse = new DdnsUpdateResponse(
                false,
                currentIp,
                request.LastKnownIp,
                "IP 未变化，无需更新");
            return Ok(ApiResponse<DdnsUpdateResponse>.Ok(noChangeResponse));
        }

        // 创建 DNS Provider
        var config = new DnsProviderConfig(
            request.ProviderId,
            request.ProviderSecret,
            request.ExtraParams);
        var provider = _providerFactory.CreateProvider(request.ProviderName, config);

        if (provider == null)
        {
            return NotFound(ApiResponse<DdnsUpdateResponse>.Fail(
                $"Provider '{request.ProviderName}' not found"));
        }

        // 更新 DNS 记录
        var updateResult = await provider.UpdateRecordAsync(
            request.Domain,
            request.RecordId,
            currentIp,
            request.Ttl,
            ct);

        if (!updateResult.Success)
        {
            return BadRequest(ApiResponse<DdnsUpdateResponse>.Fail(
                updateResult.ErrorMessage ?? "更新 DNS 记录失败"));
        }

        _logger.LogInformation(
            "成功更新 DNS 记录: {Domain} {RecordId} {OldIp} -> {NewIp}",
            request.Domain,
            request.RecordId,
            request.LastKnownIp,
            currentIp);

        var response = new DdnsUpdateResponse(
            true,
            currentIp,
            request.LastKnownIp,
            "DNS 记录更新成功");

        return Ok(ApiResponse<DdnsUpdateResponse>.Ok(response));
    }
}

// Request/Response models
public record DdnsUpdateRequest(
    string ProviderName,
    string ProviderId,
    string ProviderSecret,
    string Domain,
    string RecordId,
    string? LastKnownIp = null,
    int? Ttl = null,
    bool ForceUpdate = false,
    Dictionary<string, string>? ExtraParams = null);

public record DdnsIpResponse(string Ip, string Source);

public record IpSourceResponse(string Id, string Name, bool SupportsIpv6);

public record DdnsUpdateResponse(
    bool Updated,
    string CurrentIp,
    string? PreviousIp,
    string Message);
