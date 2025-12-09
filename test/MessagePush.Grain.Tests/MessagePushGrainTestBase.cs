using Orleans.TestingHost;

namespace MessagePush.Grain.Tests;

public class MessagePushGrainTestBase :MessagePushTestBase<MessagePushGrainTestModule>
{
    protected readonly TestCluster Cluster;

    public MessagePushGrainTestBase()
    {
        Cluster = GetRequiredService<ClusterFixture>().Cluster;

    }
}