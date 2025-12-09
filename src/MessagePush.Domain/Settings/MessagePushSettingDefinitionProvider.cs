using Volo.Abp.Settings;

namespace MessagePush.Settings;

public class MessagePushSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(MessagePushSettings.MySetting1));
    }
}
