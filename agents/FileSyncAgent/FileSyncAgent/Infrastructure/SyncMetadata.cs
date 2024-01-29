namespace FileSyncAgent.Infrastructure;

public class SyncMetadata : IEquatable<SyncMetadata>
{
    public DateTimeOffset Timestamp { get; set; }
    public string Hash { get; set; }

    public bool Equals(SyncMetadata? other)
    {
        return
            Timestamp == other?.Timestamp &&
            Hash == other?.Hash;
    }
}
