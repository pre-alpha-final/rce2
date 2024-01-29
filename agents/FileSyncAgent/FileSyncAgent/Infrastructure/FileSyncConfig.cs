namespace FileSyncAgent.Infrastructure;

public class FileSyncConfig
{
    public string FilePath { get; set; }
    public List<SyncMetadata> Syncs { get; set; } = new List<SyncMetadata>();
    public List<SyncMetadata> SyncsDescending => Syncs.OrderByDescending(e => e.Timestamp).ToList();
}
