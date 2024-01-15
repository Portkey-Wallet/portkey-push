using System.ComponentModel.DataAnnotations;

namespace MessagePush.DeviceInfo.Dtos;

public class ReportAppStatusDto
{
    [Required] public string UserId { get; set; }
    [Required] public string DeviceId { get; set; }
    [Required]  public AppStatus Status { get; set; }
    [Required] public NetworkType NetworkType { get; set; }
    public int UnreadCount { get; set; }
}