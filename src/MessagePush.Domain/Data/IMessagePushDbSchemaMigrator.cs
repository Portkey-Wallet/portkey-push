using System.Threading.Tasks;

namespace MessagePush.Data;

public interface IMessagePushDbSchemaMigrator
{
    Task MigrateAsync();
}
