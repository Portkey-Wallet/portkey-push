using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace MessagePush.Data;

/* This is used if database provider does't define
 * IIMDbSchemaMigrator implementation.
 */
public class NullMessagePushDbSchemaMigrator : IMessagePushDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
