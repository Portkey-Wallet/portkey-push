using System.ComponentModel.DataAnnotations;

namespace MessagePush.DeviceInfo.Dtos;

public class DeviceInfoDto
{
    [Required] public DeviceType DeviceType { get; set; }
    public string DeviceBrand { get; set; }
    public string OperatingSystemVersion { get; set; }
}