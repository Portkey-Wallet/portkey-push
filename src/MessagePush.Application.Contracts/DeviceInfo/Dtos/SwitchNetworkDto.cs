using System.ComponentModel.DataAnnotations;

namespace MessagePush.DeviceInfo.Dtos;

public class SwitchNetworkDto
{
    [Required] public string UserId { get; set; }
    [Required] public string DeviceId { get; set; }
    [Required] public NetworkType NetworkType { get; set; }
}