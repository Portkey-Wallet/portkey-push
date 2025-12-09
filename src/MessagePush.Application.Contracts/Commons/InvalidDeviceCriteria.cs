using System;
using System.Collections.Generic;
using MessagePush.DeviceInfo.Dtos;

namespace MessagePush.Commons;

public class InvalidDeviceCriteria
{
    public string UserId { get; set; }
    public List<string> LoginUserIds { get; set; }
    public string DeviceId { get; set; }

    public static InvalidDeviceCriteria FromUserDeviceInfoDto(UserDeviceInfoDto input)
    {
        return new InvalidDeviceCriteria
        {
            UserId = input.UserId,
            LoginUserIds = input.LoginUserIds ?? new List<string>(),
            DeviceId = input.DeviceId
        };
    }

    public static InvalidDeviceCriteria FromReportAppStatusDto(ReportAppStatusDto input)
    {
        return new InvalidDeviceCriteria
        {
            UserId = input.UserId,
            LoginUserIds = input.LoginUserIds ?? new List<string>(),
            DeviceId = input.DeviceId
        };
    }
}