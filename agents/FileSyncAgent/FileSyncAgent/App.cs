using FileSyncAgent.Extensions;
using FileSyncAgent.Helpers;
using FileSyncAgent.Infrastructure;
using FileSyncAgent.Services;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Rce2;
using System.Security.Cryptography;

namespace FileSyncAgent;

public class App : IHostedService
{
    private readonly Rce2Service _rce2Service;
    private readonly IConfigRepository _configRepository;
    private readonly SemaphoreSlim _fileWorkSemaphoreSlim = new(1, 1);

    public App(Rce2Service rce2Service, IConfigRepository configRepository)
    {
        _rce2Service = rce2Service;
        _configRepository = configRepository;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _rce2Service.Run();
        _rce2Service.SetUpdateReceivedCallback(UpdateReceivedCallback);

        while (true)
        {
            using (await _fileWorkSemaphoreSlim.DisposableWaitAsync(TimeSpan.MaxValue))
            {
                var fileSyncConfig = await _configRepository.Load();
                if (HasChanged(fileSyncConfig))
                {
                    await Update(fileSyncConfig);
                }
            }
            await Task.Delay(1000);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // ignore
    }

    private bool HasChanged(FileSyncConfig fileSyncConfig)
    {
        if (File.Exists(fileSyncConfig.FilePath) == false)
        {
            return false;
        }

        var lastHash = fileSyncConfig.SyncsDescending.FirstOrDefault()?.Hash;
        if (lastHash == null)
        {
            return true;
        }

        using SHA256 sha256Hash = SHA256.Create();
        var currentHash = sha256Hash.ComputeHash(File.ReadAllBytes(fileSyncConfig.FilePath)).ToUtf8String();

        return currentHash != lastHash;
    }

    private async Task Update(FileSyncConfig fileSyncConfig)
    {
        var newContent = File.ReadAllBytes(fileSyncConfig.FilePath);
        using SHA256 sha256Hash = SHA256.Create();
        var newHash = sha256Hash.ComputeHash(newContent).ToUtf8String();

        var timestamp = DateTimeOffset.UtcNow;
        fileSyncConfig.Syncs.Add(new SyncMetadata
        {
            Timestamp = timestamp,
            Hash = newHash,
        });
        await _configRepository.Save(fileSyncConfig);
        Console.WriteLine($"New update sent - Hash: '{newHash}', Timestamp: '{timestamp}'");

        await _rce2Service.SendUpdate(new FileSyncPayload
        {
            Syncs = fileSyncConfig.Syncs,
            Data = JsonConvert.SerializeObject(GZipHelpers.GZip(newContent)),
        });
    }

    private async Task UpdateReceivedCallback(List<SyncMetadata> syncMetadata, string base64FileContents)
    {
        using var _ = await _fileWorkSemaphoreSlim.DisposableWaitAsync(TimeSpan.MaxValue);

        var fileSyncConfig = await _configRepository.Load();
        var myLastSyncMetadata = fileSyncConfig.SyncsDescending.FirstOrDefault();
        var latestReceivedSyncData = syncMetadata.OrderByDescending(e => e.Timestamp).First();

        if (myLastSyncMetadata == null)
        {
            await ApplyReceived(latestReceivedSyncData, base64FileContents);
            Console.WriteLine($"Applied received content - Hash: '{latestReceivedSyncData.Hash}', Timestamp: '{latestReceivedSyncData.Timestamp}'");
            return;
        }

        if (syncMetadata.Contains(myLastSyncMetadata))
        {
            await ApplyReceived(latestReceivedSyncData, base64FileContents);
            Console.WriteLine($"Applied received content - Hash: '{latestReceivedSyncData.Hash}', Timestamp: '{latestReceivedSyncData.Timestamp}'");
            return;
        }

        Console.WriteLine($"Conflict detected - update not applied");
    }

    private async Task ApplyReceived(SyncMetadata latestReceivedSyncData, string base64FileContents)
    {
        var fileSyncConfig = await _configRepository.Load();
        var myLastSyncMetadata = fileSyncConfig.SyncsDescending.FirstOrDefault();

        if (File.Exists(fileSyncConfig.FilePath))
        {
            var oldContent = File.ReadAllBytes(fileSyncConfig.FilePath);
            using SHA256 sha256Hash = SHA256.Create();
            if (myLastSyncMetadata?.Hash != null &&
                myLastSyncMetadata.Hash != sha256Hash.ComputeHash(oldContent).ToUtf8String())
            {
                Console.WriteLine($"File swap detected - update not applied");
                return;
            }
        }

        await File.WriteAllBytesAsync(fileSyncConfig.FilePath, GZipHelpers.GUnZip(JsonConvert.DeserializeObject<byte[]>(base64FileContents)));
        fileSyncConfig.Syncs.Add(latestReceivedSyncData);
        await _configRepository.Save(fileSyncConfig);
    }
}
