using MessagePush.MongoDb;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;

namespace MessagePush.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(MessagePushMongoDbModule),
    typeof(MessagePushApplicationContractsModule)
    )]
public class MessagePushDbMigratorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = false);
    }
}
