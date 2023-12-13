namespace Broker.Server.Infrastructure;

public class BrokerFeedUpdate
{
    public Guid? FeedId { get; set; }
    public bool AllFeeds => FeedId == null;
}
