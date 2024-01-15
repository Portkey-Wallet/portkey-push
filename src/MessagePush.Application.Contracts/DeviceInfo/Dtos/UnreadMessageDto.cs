using System.ComponentModel.DataAnnotations;

namespace MessagePush.DeviceInfo.Dtos;

public class UnreadMessageDto
{
    [Required] public string UserId { get; set; }
    [Required] public NetworkType NetworkType { get; set; }
    public int UnreadCount { get; set; }
}