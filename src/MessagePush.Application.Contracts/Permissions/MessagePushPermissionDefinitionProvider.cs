using MessagePush.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace MessagePush.Permissions;

public class MessagePushPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        context.AddGroup(MessagePushPermissions.GroupName);
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MessagePushResource>(name);
    }
}