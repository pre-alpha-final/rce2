using BackupRestoreAgent.Config;
using BackupRestoreAgent.Services;
using Microsoft.Extensions.Hosting;
using PubSub;
using Rce2;

namespace BackupRestoreAgent;

public class App : IHostedService
{
    // Stable identity for this long-lived agent.
    private static readonly Guid AgentId = Guid.NewGuid();

    private readonly Rce2Service _rce2Service;
    private readonly ConfigRepository _configRepository;
    private readonly BackupService _backupService;

    public App(Rce2Service rce2Service, ConfigRepository configRepository, BackupService backupService)
    {
        _rce2Service = rce2Service;
        _configRepository = configRepository;
        _backupService = backupService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _rce2Service
            .SetBrokerAddress("https://localhost:7113")
            .SetAgentId(AgentId)
            .SetAgentKey(string.Empty)
            .SetAgentName("BackupRestore")
            .SetInputDefinitions(new()
            {
                { "set-path", Rce2Types.String },
                { "backup", Rce2Types.Void },
                { "restore", Rce2Types.Void },
            })
            .SetOutputDefinitions(new()
            {
                { "out", Rce2Types.String },
            })
            .Init();

        Hub.Default.Subscribe<Rce2Message>(this, async e =>
        {
            switch (e.Contact)
            {
                case "set-path":
                    await HandleSetPath(e);
                    break;
                case "backup":
                    await HandleBackup();
                    break;
                case "restore":
                    await HandleRestore();
                    break;
            }
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Hub.Default.Unsubscribe(this);
        return Task.CompletedTask;
    }

    private async Task HandleSetPath(Rce2Message e)
    {
        try
        {
            var path = e.Payload?["data"]?.ToObject<string>();
            if (string.IsNullOrWhiteSpace(path))
            {
                await Out("set-path failed: empty path");
                return;
            }

            var config = _configRepository.Load();
            config.SetPath = path;
            _configRepository.Save(config);

            await Out($"path set to '{path}'");
        }
        catch (Exception ex)
        {
            await Out($"set-path error: {ex.Message}");
        }
    }

    private async Task HandleBackup()
    {
        try
        {
            var path = _configRepository.Load().SetPath;
            var result = await Task.Run(() => _backupService.Backup(path!));
            await Out(result);
        }
        catch (Exception ex)
        {
            await Out($"backup error: {ex.Message}");
        }
    }

    private async Task HandleRestore()
    {
        try
        {
            var path = _configRepository.Load().SetPath;
            var result = await Task.Run(() => _backupService.Restore(path!));
            await Out(result);
        }
        catch (Exception ex)
        {
            await Out($"restore error: {ex.Message}");
        }
    }

    private async Task Out(string message)
    {
        Console.WriteLine(message);
        await _rce2Service.Send("out", message);
    }
}
