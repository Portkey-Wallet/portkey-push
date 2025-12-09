using System.Threading.Tasks;
using MessagePush.MessagePush.Dtos;

namespace MessagePush.MessagePush;

public interface IMessagePushAppService
{
    Task PushMessageAsync(MessagePushDto input);
    Task ClearMessageAsync(ClearMessageDto input);
}