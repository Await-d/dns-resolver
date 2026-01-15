namespace DnsResolver.Infrastructure.BackgroundServices;

using DnsResolver.Application.Services;
using DnsResolver.Domain.Aggregates.DdnsTask;
using DnsResolver.Domain.Services;
using DnsResolver.Infrastructure.DnsProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// 后台服务：定时检查并更新 DDNS 任务
/// </summary>
public class DdnsSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DdnsSchedulerService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public DdnsSchedulerService(
        IServiceProvider serviceProvider,
        ILogger<DdnsSchedulerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DDNS Scheduler Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDdnsTasksAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing DDNS tasks");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("DDNS Scheduler Service stopped");
    }

    private async Task ProcessDdnsTasksAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IDdnsTaskRepository>();
        var ddnsService = scope.ServiceProvider.GetRequiredService<IDdnsService>();
        var providerFactory = scope.ServiceProvider.GetRequiredService<DnsProviderFactory>();

        var tasks = await repository.GetEnabledTasksAsync(cancellationToken);

        foreach (var task in tasks)
        {
            if (!task.ShouldCheck())
                continue;

            try
            {
                await ProcessSingleTaskAsync(task, ddnsService, providerFactory, repository, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing DDNS task {TaskId} - {TaskName}", task.Id, task.Name);
                task.RecordError($"Processing error: {ex.Message}");
                await repository.UpdateAsync(task, cancellationToken);
            }
        }
    }

    private async Task ProcessSingleTaskAsync(
        DdnsTask task,
        IDdnsService ddnsService,
        DnsProviderFactory providerFactory,
        IDdnsTaskRepository repository,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking DDNS task {TaskId} - {TaskName}", task.Id, task.Name);

        // 获取当前公网 IP
        var ipResult = await ddnsService.GetCurrentPublicIpAsync(cancellationToken);
        task.RecordCheck();

        if (!ipResult.Success || string.IsNullOrEmpty(ipResult.Ip))
        {
            var error = ipResult.ErrorMessage ?? "Failed to get public IP";
            _logger.LogWarning("Failed to get IP for task {TaskId}: {Error}", task.Id, error);
            task.RecordError(error);
            await repository.UpdateAsync(task, cancellationToken);
            return;
        }

        var currentIp = ipResult.Ip;

        // 检查 IP 是否变化
        if (currentIp == task.LastKnownIp)
        {
            _logger.LogDebug("IP unchanged for task {TaskId}: {Ip}", task.Id, currentIp);
            await repository.UpdateAsync(task, cancellationToken);
            return;
        }

        _logger.LogInformation(
            "IP changed for task {TaskId}: {OldIp} -> {NewIp}",
            task.Id, task.LastKnownIp ?? "none", currentIp);

        // 创建 DNS Provider 并更新记录
        var config = new DnsProviderConfig(
            task.ProviderId,
            task.ProviderSecret,
            task.ExtraParams);

        var provider = providerFactory.CreateProvider(task.ProviderName, config);
        if (provider == null)
        {
            var error = $"Provider '{task.ProviderName}' not found";
            _logger.LogError("Failed to create provider for task {TaskId}: {Error}", task.Id, error);
            task.RecordError(error);
            await repository.UpdateAsync(task, cancellationToken);
            return;
        }

        // 更新 DNS 记录
        var updateResult = await provider.UpdateRecordAsync(
            task.Domain,
            task.RecordId,
            currentIp,
            task.Ttl,
            cancellationToken);

        if (updateResult.Success)
        {
            _logger.LogInformation(
                "Successfully updated DNS record for task {TaskId}: {Domain} -> {Ip}",
                task.Id, task.Domain, currentIp);
            task.UpdateIp(currentIp);
        }
        else
        {
            var error = updateResult.ErrorMessage ?? "Update failed";
            _logger.LogError(
                "Failed to update DNS record for task {TaskId}: {Error}",
                task.Id, error);
            task.RecordError(error);
        }

        await repository.UpdateAsync(task, cancellationToken);
    }
}
