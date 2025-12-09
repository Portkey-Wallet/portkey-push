using MessagePush.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace MessagePush.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class MessagePushBaseController : AbpControllerBase
{
    protected MessagePushBaseController()
    {
        LocalizationResource = typeof(MessagePushResource);
    }
}