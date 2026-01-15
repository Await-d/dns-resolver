using Microsoft.AspNetCore.Mvc;
using DnsResolver.Api.Requests;
using DnsResolver.Api.Responses;
using DnsResolver.Application.Commands.ResolveDns;
using DnsResolver.Application.Commands.CompareDns;
using DnsResolver.Application.Queries.GetIsps;
using DnsResolver.Application.DTOs;

namespace DnsResolver.Api.Controllers;

[ApiController]
[Route("api/v1/dns")]
public class DnsController : ControllerBase
{
    private readonly ResolveDnsCommandHandler _resolveHandler;
    private readonly CompareDnsCommandHandler _compareHandler;
    private readonly GetIspsQueryHandler _getIspsHandler;

    public DnsController(
        ResolveDnsCommandHandler resolveHandler,
        CompareDnsCommandHandler compareHandler,
        GetIspsQueryHandler getIspsHandler)
    {
        _resolveHandler = resolveHandler;
        _compareHandler = compareHandler;
        _getIspsHandler = getIspsHandler;
    }

    /// <summary>
    /// 获取所有支持的运营商列表
    /// </summary>
    [HttpGet("isps")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<IspProviderDto>>>> GetIsps(
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
