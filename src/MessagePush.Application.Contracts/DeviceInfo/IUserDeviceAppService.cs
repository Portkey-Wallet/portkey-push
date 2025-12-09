using System.Threading.Tasks;
using MessagePush.DeviceInfo.Dtos;

namespace MessagePush.DeviceInfo;

public interface IUserDeviceAppService
{
    Task ReportDeviceInfoAsync(UserDeviceInfoDto input);
    Task ReportAppStatusAsync(ReportAppStatusDto input);
    Task ExitWalletAsync(ExitWalletDto input);
    Task SwitchNetworkAsync(SwitchNetworkDto input);
    Task UpdateUnreadMessageAsync(UnreadMessageDto input);
}