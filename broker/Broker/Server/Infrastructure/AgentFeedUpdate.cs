namespace Broker.Server.Infrastructure;

public class AgentFeedUpdate
{
    public Guid? FeedId { get; set; }
    public bool AllFeeds => FeedId == null;
}
