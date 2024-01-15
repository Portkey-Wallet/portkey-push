using System.Threading.Tasks;
using MessagePush.DeviceInfo;
using MessagePush.DeviceInfo.Dtos;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace MessagePush.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("MessagePushUser")]
[Route("api/v1/userDevice")]
[IgnoreAntiforgeryToken]
public class UserDeviceController : MessagePushBaseController
{
    private readonly IUserDeviceAppService _userDeviceAppService;

    public UserDeviceController(IUserDeviceAppService userDeviceAppService)
    {
        _userDeviceAppService = userDeviceAppService;
    }

    [HttpPost("reportDeviceInfo")]
    public async Task ReportDeviceInfoAsync(UserDeviceInfoDto input)
    {
        await _userDeviceAppService.ReportDeviceInfoAsync(input);
    }

    [HttpPost("reportAppStatus")]
    public async Task ReportAppStatusAsync(ReportAppStatusDto input)
    {
        await _userDeviceAppService.ReportAppStatusAsync(input);
    }
    
    [HttpPost("exitWallet")]
    public async Task ExitWalletAsync(ExitWalletDto input)
    {
        await _userDeviceAppService.ExitWalletAsync(input);
    }
    
    [HttpPost("switchNetwork")]
    public async Task SwitchNetworkAsync(SwitchNetworkDto input)
    {
        await _userDeviceAppService.SwitchNetworkAsync(input);
    }

    [HttpPost("updateUnreadMessage")]
    public async Task UpdateUnreadMessageAsync(UnreadMessageDto input)
    {
        await _userDeviceAppService.UpdateUnreadMessageAsync(input);
    }
}