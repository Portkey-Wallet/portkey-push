using System;
using Volo.Abp;

namespace MessagePush.Commons;

public static class DeviceInfoHelper
{
    public static string GetId(string userId, string deviceId, string network)
    {
        if (userId.IsNullOrEmpty() || deviceId.IsNullOrEmpty() || network.IsNullOrEmpty())
        {
            throw new UserFriendlyException("invalid input");
        }

        return $"{userId}-{deviceId}-{network}";
    }
}