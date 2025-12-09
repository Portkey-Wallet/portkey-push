using System.Collections.Generic;

namespace MessagePush.MessagePush.Dtos;

public class ClearMessageDto
{
    public string UserId { get; set; }
    public string DeviceId { get; set; }
    public string AppId { get; set; }
    public Dictionary<string,string> Data { get; set; }
}