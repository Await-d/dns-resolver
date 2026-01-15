namespace DnsResolver.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using DnsResolver.Api.Requests;
using DnsResolver.Api.Responses;
using DnsResolver.Domain.Services;
using DnsResolver.Infrastructure.DnsProviders;

[ApiController]
[Route("api/v1/providers")]
public class DnsProviderController : ControllerBase
{
    private readonly DnsProviderFactory _providerFactory;
    private readonly Dictionary<string, IDnsProvider> _configuredProviders = new();

    public DnsProviderController(DnsProviderFactory providerFactory)
    {
        _providerFactory = providerFactory;
    }

    /// <summary>
    /// 获取所有支持的 DNS 服务商列表
    /// </summary>
    [HttpGet]
    public ActionResult<ApiResponse<IEnumerable<ProviderInfo>>> GetProviders()
    {
        var providers = _providerFactory.GetProviderInfos()
            .Select(p => new ProviderInfo(p.Name, p.DisplayName))
            .ToList();
        return Ok(ApiResponse<IEnumerable<ProviderInfo>>.Ok(providers));
    }

    /// <summary>
    /// 配置 DNS 服务商凭证
    /// </summary>
    [HttpPost("configure")]
    public ActionResult<ApiResponse<bool>> ConfigureProvider([FromBody] ConfigureProviderRequest request)
    {
        var config = new DnsProviderConfig(request.Id, request.Secret, request.ExtraParams);
        var provider = _providerFactory.CreateProvider(request.ProviderName, config);

        if (provider == null)
            return NotFound(ApiResponse<bool>.Fail($"Provider '{request.ProviderName}' not found"));

        return Ok(ApiResponse<bool>.Ok(true));
    }

    /// <summary>
    /// 获取域名列表
    /// </summary>
    [HttpPost("domains")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<string>>>> GetDomains(
        [FromBody] ConfigureProviderRequest request,
        CancellationToken ct)
    {
        var provider = CreateConfiguredProvider(request);
        if (provider == null)
            return NotFound(ApiResponse<IReadOnlyList<string>>.Fail($"Provider '{request.ProviderName}' not found"));

        var result = await provider.GetDomainsAsync(ct);
        if (!result.Success)
            return BadRequest(ApiResponse<IReadOnlyList<string>>.Fail(result.ErrorMessage ?? "Failed"));

        return Ok(ApiResponse<IReadOnlyList<string>>.Ok(result.Data!));
    }

    /// <summary>
    /// 获取 DNS 记录列表
    /// </summary>
    [HttpPost("records/list")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DnsRecordInfo>>>> GetRecords(
        [FromBody] GetRecordsWithCredentialsRequest request,
        CancellationToken ct)
    {
        var provider = CreateConfiguredProvider(request);
        if (provider == null)
            return NotFound(ApiResponse<IReadOnlyList<DnsRecordInfo>>.Fail($"Provider '{request.ProviderName}' not found"));

        var result = await provider.GetRecordsAsync(request.Domain, request.SubDomain, request.RecordType, ct);
        if (!result.Success)
            return BadRequest(ApiResponse<IReadOnlyList<DnsRecordInfo>>.Fail(result.ErrorMessage ?? "Failed"));

        return Ok(ApiResponse<IReadOnlyList<DnsRecordInfo>>.Ok(result.Data!));
    }

    /// <summary>
    /// 添加 DNS 记录
    /// </summary>
    [HttpPost("records/add")]
    public async Task<ActionResult<ApiResponse<DnsRecordInfo>>> AddRecord(
        [FromBody] AddRecordWithCredentialsRequest request,
        CancellationToken ct)
    {
        var provider = CreateConfiguredProvider(request);
        if (provider == null)
            return NotFound(ApiResponse<DnsRecordInfo>.Fail($"Provider '{request.ProviderName}' not found"));

        var result = await provider.AddRecordAsync(request.Domain, request.SubDomain, request.RecordType, request.Value, request.Ttl, ct);
        if (!result.Success)
            return BadRequest(ApiResponse<DnsRecordInfo>.Fail(result.ErrorMessage ?? "Failed"));

        return Ok(ApiResponse<DnsRecordInfo>.Ok(result.Data!));
    }

    /// <summary>
    /// 更新 DNS 记录
    /// </summary>
    [HttpPost("records/update")]
    public async Task<ActionResult<ApiResponse<DnsRecordInfo>>> UpdateRecord(
        [FromBody] UpdateRecordWithCredentialsRequest request,
        CancellationToken ct)
    {
        var provider = CreateConfiguredProvider(request);
        if (provider == null)
            return NotFound(ApiResponse<DnsRecordInfo>.Fail($"Provider '{request.ProviderName}' not found"));

        var result = await provider.UpdateRecordAsync(request.Domain, request.RecordId, request.Value, request.Ttl, ct);
        if (!result.Success)
            return BadRequest(ApiResponse<DnsRecordInfo>.Fail(result.ErrorMessage ?? "Failed"));

        return Ok(ApiResponse<DnsRecordInfo>.Ok(result.Data!));
    }

    /// <summary>
    /// 删除 DNS 记录
    /// </summary>
    [HttpPost("records/delete")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteRecord(
        [FromBody] DeleteRecordWithCredentialsRequest request,
        CancellationToken ct)
    {
        var provider = CreateConfiguredProvider(request);
        if (provider == null)
            return NotFound(ApiResponse<bool>.Fail($"Provider '{request.ProviderName}' not found"));

        var result = await provider.DeleteRecordAsync(request.Domain, request.RecordId, ct);
        if (!result.Success)
            return BadRequest(ApiResponse<bool>.Fail(result.ErrorMessage ?? "Failed"));

        return Ok(ApiResponse<bool>.Ok(true));
    }

    /// <summary>
    /// 批量添加 DNS 记录
    /// </summary>
    [HttpPost("records/batch-add")]
    public async Task<ActionResult<ApiResponse<BatchOperationResult>>> BatchAddRecords(
        [FromBody] BatchAddRecordsRequest request,
        CancellationToken ct)
    {
        var provider = CreateConfiguredProvider(request);
        if (provider == null)
            return NotFound(ApiResponse<BatchOperationResult>.Fail($"Provider '{request.ProviderName}' not found"));

        var results = new List<RecordOperationResult>();
        var successCount = 0;
        var failureCount = 0;

        foreach (var record in request.Records)
        {
            var result = await provider.AddRecordAsync(
                request.Domain,
                record.SubDomain,
                record.RecordType,
                record.Value,
                record.Ttl,
                ct);

            if (result.Success && result.Data != null)
            {
                successCount++;
                results.Add(new RecordOperationResult(
                    true,
                    record.SubDomain,
                    record.RecordType,
                    result.Data.RecordId,
                    null));
            }
            else
            {
                failureCount++;
                results.Add(new RecordOperationResult(
                    false,
                    record.SubDomain,
                    record.RecordType,
                    null,
                    result.ErrorMessage ?? "Failed"));
            }
        }

        var batchResult = new BatchOperationResult(successCount, failureCount, results);
        return Ok(ApiResponse<BatchOperationResult>.Ok(batchResult));
    }

    /// <summary>
    /// 批量删除 DNS 记录
    /// </summary>
    [HttpPost("records/batch-delete")]
    public async Task<ActionResult<ApiResponse<BatchOperationResult>>> BatchDeleteRecords(
        [FromBody] BatchDeleteRecordsRequest request,
        CancellationToken ct)
    {
        var provider = CreateConfiguredProvider(request);
        if (provider == null)
            return NotFound(ApiResponse<BatchOperationResult>.Fail($"Provider '{request.ProviderName}' not found"));

        var results = new List<RecordOperationResult>();
        var successCount = 0;
        var failureCount = 0;

        foreach (var recordId in request.RecordIds)
        {
            var result = await provider.DeleteRecordAsync(request.Domain, recordId, ct);

            if (result.Success)
            {
                successCount++;
                results.Add(new RecordOperationResult(true, null, null, recordId, null));
            }
            else
            {
                failureCount++;
                results.Add(new RecordOperationResult(false, null, null, recordId, result.ErrorMessage ?? "Failed"));
            }
        }

        var batchResult = new BatchOperationResult(successCount, failureCount, results);
        return Ok(ApiResponse<BatchOperationResult>.Ok(batchResult));
    }

    private IDnsProvider? CreateConfiguredProvider(ConfigureProviderRequest request)
    {
        var config = new DnsProviderConfig(request.Id, request.Secret, request.ExtraParams);
        return _providerFactory.CreateProvider(request.ProviderName, config);
    }
}

public record ProviderInfo(string Name, string DisplayName);

// Extended request types with credentials
public record GetRecordsWithCredentialsRequest(
    string ProviderName,
    string Id,
    string Secret,
    string Domain,
    string? SubDomain = null,
    string? RecordType = null,
    Dictionary<string, string>? ExtraParams = null) : ConfigureProviderRequest(ProviderName, Id, Secret, ExtraParams);

public record AddRecordWithCredentialsRequest(
    string ProviderName,
    string Id,
    string Secret,
    string Domain,
    string SubDomain,
    string RecordType,
    string Value,
    int Ttl = 600,
    Dictionary<string, string>? ExtraParams = null) : ConfigureProviderRequest(ProviderName, Id, Secret, ExtraParams);

public record UpdateRecordWithCredentialsRequest(
    string ProviderName,
    string Id,
    string Secret,
    string Domain,
    string RecordId,
    string Value,
    int? Ttl = null,
    Dictionary<string, string>? ExtraParams = null) : ConfigureProviderRequest(ProviderName, Id, Secret, ExtraParams);

public record DeleteRecordWithCredentialsRequest(
    string ProviderName,
    string Id,
    string Secret,
    string Domain,
    string RecordId,
    Dictionary<string, string>? ExtraParams = null) : ConfigureProviderRequest(ProviderName, Id, Secret, ExtraParams);

// Batch operation request types
public record BatchAddRecordsRequest(
    string ProviderName,
    string Id,
    string Secret,
    string Domain,
    List<BatchRecordItem> Records,
    Dictionary<string, string>? ExtraParams = null) : ConfigureProviderRequest(ProviderName, Id, Secret, ExtraParams);

public record BatchDeleteRecordsRequest(
    string ProviderName,
    string Id,
    string Secret,
    string Domain,
    List<string> RecordIds,
    Dictionary<string, string>? ExtraParams = null) : ConfigureProviderRequest(ProviderName, Id, Secret, ExtraParams);

public record BatchRecordItem(
    string SubDomain,
    string RecordType,
    string Value,
    int Ttl = 600);

public record BatchOperationResult(
    int SuccessCount,
    int FailureCount,
    List<RecordOperationResult> Results);

public record RecordOperationResult(
    bool Success,
    string? SubDomain,
    string? RecordType,
    string? RecordId,
    string? ErrorMessage);
