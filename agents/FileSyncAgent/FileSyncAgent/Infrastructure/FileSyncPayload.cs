namespace FileSyncAgent.Infrastructure;

public class FileSyncPayload
{
    public List<SyncMetadata> Syncs { get; set; } = new List<SyncMetadata>();
    public string Data { get; set; }
}
