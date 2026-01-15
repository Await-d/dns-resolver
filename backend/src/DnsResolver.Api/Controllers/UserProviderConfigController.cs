namespace DnsResolver.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DnsResolver.Api.Responses;
using DnsResolver.Domain.Aggregates.UserProviderConfig;
using DnsResolver.Domain.Aggregates.User;
using DnsResolver.Domain.Services;
using DnsResolver.Infrastructure.DnsProviders;
using System.Security.Claims;

[ApiController]
[Route("api/v1/user/providers")]
[Authorize]
public class UserProviderConfigController : ControllerBase
{
    private readonly IUserProviderConfigRepository _configRepository;
    private readonly IUserRepository _userRepository;
    private readonly DnsProviderFactory _providerFactory;

    public UserProviderConfigController(
        IUserProviderConfigRepository configRepository,
        IUserRepository userRepository,
        DnsProviderFactory providerFactory)
    {
        _configRepository = configRepository;
        _userRepository = userRepository;
        _providerFactory = providerFactory;
    }

    /// <summary>
    /// 获取当前用户已配置的服务商列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserProviderConfigDto>>>> GetUserConfigs(CancellationToken ct)
    {
        var userId = await GetCurrentUserIdAsync(ct);
        if (userId == null)
            return Unauthorized(ApiResponse<IReadOnlyList<UserProviderConfigDto>>.Fail("User not found"));

        var configs = await _configRepository.GetByUserIdAsync(userId.Value, ct);
        var dtos = configs.Select(c => new UserProviderConfigDto(
            c.Id,
            c.ProviderName,
            c.DisplayName,
            c.IsActive,
            c.CreatedAt,
            c.LastUsedAt
        )).ToList();

        return Ok(ApiResponse<IReadOnlyList<UserProviderConfigDto>>.Ok(dtos));
    }

    /// <summary>
    /// 获取所有可用的服务商列表（包含用户是否已配置的信息）
    /// </summary>
    [HttpGet("available")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AvailableProviderDto>>>> GetAvailableProviders(CancellationToken ct)
    {
        var userId = await GetCurrentUserIdAsync(ct);
        if (userId == null)
            return Unauthorized(ApiResponse<IReadOnlyList<AvailableProviderDto>>.Fail("User not found"));

        var allProviders = _providerFactory.GetProviderInfos().ToList();
        var userConfigs = await _configRepository.GetByUserIdAsync(userId.Value, ct);
        var configuredProviders = userConfigs.ToDictionary(c => c.ProviderName, c => c);

        var dtos = allProviders.Select(p =>
        {
            var isConfigured = configuredProviders.TryGetValue(p.Name, out var config);
            return new AvailableProviderDto(
                p.Name,
                p.DisplayName,
                isConfigured,
                isConfigured ? config!.Id : null,
                isConfigured && config!.IsActive
            );
        }).ToList();

        return Ok(ApiResponse<IReadOnlyList<AvailableProviderDto>>.Ok(dtos));
    }

    /// <summary>
    /// 添加服务商配置
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserProviderConfigDto>>> AddConfig(
        [FromBody] AddUserProviderConfigRequest request,
        CancellationToken ct)
    {
        var userId = await GetCurrentUserIdAsync(ct);
        if (userId == null)
            return Unauthorized(ApiResponse<UserProviderConfigDto>.Fail("User not found"));

        // 检查服务商是否存在
        var providerInfo = _providerFactory.GetProviderInfos().FirstOrDefault(p => p.Name == request.ProviderName);
        if (providerInfo == null)
            return NotFound(ApiResponse<UserProviderConfigDto>.Fail($"Provider '{request.ProviderName}' not found"));

        // 检查是否已配置
        var existing = await _configRepository.GetByUserAndProviderAsync(userId.Value, request.ProviderName, ct);
        if (existing != null)
            return BadRequest(ApiResponse<UserProviderConfigDto>.Fail($"Provider '{request.ProviderName}' already configured"));

        // 验证凭证是否有效
        var validationResult = await ValidateCredentialsAsync(request.ProviderName, request.ApiId, request.ApiSecret, request.ExtraParams, ct);
        if (!validationResult.Success)
            return BadRequest(ApiResponse<UserProviderConfigDto>.Fail($"Invalid credentials: {validationResult.ErrorMessage}"));

        var config = UserProviderConfig.Create(
            userId.Value,
            request.ProviderName,
            request.DisplayName ?? providerInfo.DisplayName,
            request.ApiId,
            request.ApiSecret,
            request.ExtraParams
        );

        await _configRepository.AddAsync(config, ct);

        var dto = new UserProviderConfigDto(
            config.Id,
            config.ProviderName,
            config.DisplayName,
            config.IsActive,
            config.CreatedAt,
            config.LastUsedAt
        );

        return Ok(ApiResponse<UserProviderConfigDto>.Ok(dto));
    }

    /// <summary>
    /// 更新服务商配置
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserProviderConfigDto>>> UpdateConfig(
        Guid id,
        [FromBody] UpdateUserProviderConfigRequest request,
        CancellationToken ct)
    {
        var userId = await GetCurrentUserIdAsync(ct);
        if (userId == null)
            return Unauthorized(ApiResponse<UserProviderConfigDto>.Fail("User not found"));

        var config = await _configRepository.GetByIdAsync(id, ct);
        if (config == null || config.UserId != userId.Value)
            return NotFound(ApiResponse<UserProviderConfigDto>.Fail("Config not found"));

        // 验证凭证是否有效
        var validationResult = await ValidateCredentialsAsync(config.ProviderName, request.ApiId, request.ApiSecret, request.ExtraParams, ct);
        if (!validationResult.Success)
            return BadRequest(ApiResponse<UserProviderConfigDto>.Fail($"Invalid credentials: {validationResult.ErrorMessage}"));

        config.Update(request.DisplayName, request.ApiId, request.ApiSecret, request.ExtraParams);
        await _configRepository.UpdateAsync(config, ct);

        var dto = new UserProviderConfigDto(
            config.Id,
            config.ProviderName,
            config.DisplayName,
            config.IsActive,
            config.CreatedAt,
            config.LastUsedAt
        );

        return Ok(ApiResponse<UserProviderConfigDto>.Ok(dto));
    }

    /// <summary>
    /// 删除服务商配置
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteConfig(Guid id, CancellationToken ct)
    {
        var userId = await GetCurrentUserIdAsync(ct);
        if (userId == null)
            return Unauthorized(ApiResponse<bool>.Fail("User not found"));

        var config = await _configRepository.GetByIdAsync(id, ct);
        if (config == null || config.UserId != userId.Value)
            return NotFound(ApiResponse<bool>.Fail("Config not found"));

        await _configRepository.DeleteAsync(id, ct);
        return Ok(ApiResponse<bool>.Ok(true));
    }

    /// <summary>
    /// 切换服务商配置的启用状态
    /// </summary>
    [HttpPost("{id:guid}/toggle")]
    public async Task<ActionResult<ApiResponse<UserProviderConfigDto>>> ToggleConfig(Guid id, CancellationToken ct)
    {
        var userId = await GetCurrentUserIdAsync(ct);
        if (userId == null)
            return Unauthorized(ApiResponse<UserProviderConfigDto>.Fail("User not found"));

        var config = await _configRepository.GetByIdAsync(id, ct);
        if (config == null || config.UserId != userId.Value)
            return NotFound(ApiResponse<UserProviderConfigDto>.Fail("Config not found"));

        config.SetActive(!config.IsActive);
        await _configRepository.UpdateAsync(config, ct);

        var dto = new UserProviderConfigDto(
            config.Id,
            config.ProviderName,
            config.DisplayName,
            config.IsActive,
            config.CreatedAt,
            config.LastUsedAt
        );

        return Ok(ApiResponse<UserProviderConfigDto>.Ok(dto));
    }

    private async Task<Guid?> GetCurrentUserIdAsync(CancellationToken ct)
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(username))
            return null;

        var user = await _userRepository.GetByUsernameAsync(username, ct);
        return user?.Id;
    }

    private async Task<(bool Success, string? ErrorMessage)> ValidateCredentialsAsync(
        string providerName,
        string apiId,
        string apiSecret,
        Dictionary<string, string>? extraParams,
        CancellationToken ct)
    {
        var config = new DnsProviderConfig(apiId, apiSecret, extraParams);
        var provider = _providerFactory.CreateProvider(providerName, config);
        if (provider == null)
            return (false, "Provider not found");

        try
        {
            // 尝试获取域名列表来验证凭证
            var result = await provider.GetDomainsAsync(ct);
            return (result.Success, result.ErrorMessage);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}

public record UserProviderConfigDto(
    Guid Id,
    string ProviderName,
    string DisplayName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastUsedAt
);

public record AvailableProviderDto(
    string Name,
    string DisplayName,
    bool IsConfigured,
    Guid? ConfigId,
    bool IsActive
);

public record AddUserProviderConfigRequest(
    string ProviderName,
    string ApiId,
    string ApiSecret,
    string? DisplayName = null,
    Dictionary<string, string>? ExtraParams = null
);

public record UpdateUserProviderConfigRequest(
    string DisplayName,
    string ApiId,
    string ApiSecret,
    Dictionary<string, string>? ExtraParams = null
);
