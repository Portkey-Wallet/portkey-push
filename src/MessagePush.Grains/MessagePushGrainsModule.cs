using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace MessagePush.Grains;

[DependsOn(typeof(MessagePushApplicationContractsModule),
    typeof(AbpAutoMapperModule))]
public class MessagePushGrainsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<MessagePushGrainsModule>(); });

        var configuration = context.Services.GetConfiguration();
        var connStr = configuration["GraphQL:Configuration"];
    }
}