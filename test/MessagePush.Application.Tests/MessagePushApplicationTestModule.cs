using MessagePush.Grain.Tests;
using Volo.Abp.AutoMapper;
using Volo.Abp.EventBus;
using Volo.Abp.Modularity;

namespace MessagePush;

[DependsOn(
    typeof(MessagePushApplicationModule),
    typeof(AbpEventBusModule),
    typeof(MessagePushGrainTestModule),
    typeof(MessagePushDomainTestModule)
)]
public class MessagePushApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // context.Services.AddSingleton(sp => sp.GetService<ClusterFixture>().Cluster.Client);
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<MessagePushApplicationModule>(); });
        
        base.ConfigureServices(context);
    }
}