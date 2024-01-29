using FileSyncAgent.Infrastructure;

namespace FileSyncAgent.Services;

public interface IConfigRepository
{
    Task<FileSyncConfig> Load();
    Task Save(FileSyncConfig fileSyncConfig);
}
