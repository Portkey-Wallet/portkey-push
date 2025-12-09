using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace MessagePush.EntityEventHandler.Core
{
    [DependsOn(typeof(AbpAutoMapperModule), typeof(MessagePushApplicationModule),
        typeof(MessagePushApplicationContractsModule))]
    public class MessagePushEntityEventHandlerCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAutoMapperOptions>(options =>
            {
                //Add all mappings defined in the assembly of the MyModule class
                options.AddMaps<MessagePushEntityEventHandlerCoreModule>();
            });
        }
    }
}