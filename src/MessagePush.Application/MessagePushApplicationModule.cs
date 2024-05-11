using System.Configuration;
using MessagePush.Grains;
using MessagePush.Options;
using MessagePush.Redis;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.DistributedLocking;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace MessagePush;

[DependsOn(
    typeof(MessagePushDomainModule),
    typeof(AbpAccountApplicationModule),
    typeof(MessagePushApplicationContractsModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(MessagePushGrainsModule),
    typeof(AbpDistributedLockingModule)
)]
public class MessagePushApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<MessagePushApplicationModule>(); });
        context.Services.AddHttpClient();
        context.Services.AddSingleton<RedisClient>();
        
        var configuration = context.Services.GetConfiguration();
        Configure<ScheduledTasksOptions>(configuration.GetSection("ScheduledTasks"));
        Configure<MessagePushOptions>(configuration.GetSection("MessagePush"));
    }
}