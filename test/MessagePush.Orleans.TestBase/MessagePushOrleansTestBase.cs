using Orleans.TestingHost;
using Volo.Abp.Modularity;

namespace MessagePush.Orleans.TestBase;

public abstract class MessagePushOrleansTestBase<TStartupModule> : MessagePushTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    protected readonly TestCluster Cluster;

    public MessagePushOrleansTestBase()
    {
        Cluster = GetRequiredService<ClusterFixture>().Cluster;
    }
}