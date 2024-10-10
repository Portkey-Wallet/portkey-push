using System.Threading.Tasks;
using Asp.Versioning;
using MessagePush.MessagePush;
using MessagePush.MessagePush.Dtos;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace MessagePush.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("MessagePush")]
[Route("api/v1/messagePush")]
public class MessagePushController : MessagePushBaseController
{
    private readonly IMessagePushAppService _messagePushAppService;

    public MessagePushController(IMessagePushAppService messagePushAppService)
    {
        _messagePushAppService = messagePushAppService;
    }

    [HttpPost("push")]
    public async Task PushMessageAsync(MessagePushDto input)
    {
        await _messagePushAppService.PushMessageAsync(input);
    }
    
    [HttpPost("clear")]
    public async Task ClearMessageAsync(ClearMessageDto input)
    {
        await _messagePushAppService.ClearMessageAsync(input);
    }
}