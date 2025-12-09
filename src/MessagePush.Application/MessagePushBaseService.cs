using MessagePush.Localization;
using Volo.Abp.Application.Services;

namespace MessagePush;

/* Inherit your application services from this class.
 */
public abstract class MessagePushBaseService : ApplicationService
{
    protected MessagePushBaseService()
    {
        LocalizationResource = typeof(MessagePushResource);
    }
}
