using System.ComponentModel.DataAnnotations;

namespace MessagePush.DeviceInfo.Dtos;

public class UserDeviceInfoDto
{
    [Required] public string UserId { get; set; }
    [Required] public string Token { get; set; }
    [Required] public string DeviceId { get; set; }
    public long RefreshTime { get; set; }
    [Required] public NetworkType NetworkType { get; set; }
    public DeviceInfoDto DeviceInfo { get; set; }
}