using Microsoft.AspNetCore.Mvc;
using DnsResolver.Api.Requests;
using DnsResolver.Api.Responses;
using DnsResolver.Application.Commands.ResolveDns;
using DnsResolver.Application.Commands.CompareDns;
using DnsResolver.Application.Queries.GetIsps;
using DnsResolver.Application.Queries.GetDnsProviders;
using DnsResolver.Application.DTOs;

namespace DnsResolver.Api.Controllers;

[ApiController]
[Route("api/v1/dns")]
public class DnsController : ControllerBase
{
    private readonly ResolveDnsCommandHandler _resolveHandler;
    private readonly CompareDnsCommandHandler _compareHandler;
    private readonly GetIspsQueryHandler _getIspsHandler;
    private readonly GetDnsProvidersQueryHandler _getDnsProvidersHandler;

    public DnsController(
        ResolveDnsCommandHandler resolveHandler,
        CompareDnsCommandHandler compareHandler,
        GetIspsQueryHandler getIspsHandler,
        GetDnsProvidersQueryHandler getDnsProvidersHandler)
    {
        _resolveHandler = resolveHandler;
        _compareHandler = compareHandler;
        _getIspsHandler = getIspsHandler;
        _getDnsProvidersHandler = getDnsProvidersHandler;
    }

    /// <summary>
    /// 获取所有支持的 DNS 域名服务商列表（用于域名解析配置）
    /// </summary>
    [HttpGet("isps")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DnsProviderInfoDto>>>> GetIsps(
        CancellationToken cancellationToken)
    {
        var result = await _getDnsProvidersHandler.HandleAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DnsProviderInfoDto>>.Ok(result));
    }

    /// <summary>
    /// 获取所有支持的 DNS 域名服务商列表
    /// </summary>
    [HttpGet("providers")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DnsProviderInfoDto>>>> GetProviders(
        CancellationToken cancellationToken)
    {
        var result = await _getDnsProvidersHandler.HandleAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DnsProviderInfoDto>>.Ok(result));
    }

    /// <summary>
    /// 获取 DNS 解析服务器列表（用于 DNS 查询对比）
    /// </summary>
    [HttpGet("dns-servers")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<IspProviderDto>>>> GetDnsServers(
        CancellationToken cancellationToken)
    {
        var result = await _getIspsHandler.HandleAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<IspProviderDto>>.Ok(result));
    }

    /// <summary>
    /// 单次 DNS 解析
    /// </summary>
    [HttpPost("resolve")]
    public async Task<ActionResult<ApiResponse<ResolveDnsResult>>> Resolve(
        [FromBody] ResolveRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ResolveDnsCommand(request.Domain, request.RecordType, request.DnsServer);
        var result = await _resolveHandler.HandleAsync(command, cancellationToken);
        return Ok(ApiResponse<ResolveDnsResult>.Ok(result));
    }

    /// <summary>
    /// 批量对比解析
    /// </summary>
    [HttpPost("compare")]
    public async Task<ActionResult<ApiResponse<CompareDnsResult>>> Compare(
        [FromBody] CompareRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CompareDnsCommand(request.Domain, request.RecordType, request.IspList);
        var result = await _compareHandler.HandleAsync(command, cancellationToken);
        return Ok(ApiResponse<CompareDnsResult>.Ok(result));
    }
}
