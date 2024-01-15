using System;
using System.Collections.Generic;
using System.Linq;
using MessagePush.DeviceInfo;
using MessagePush.Entities.Es;
using Volo.Abp;

namespace MessagePush.Common;

public static class UnreadMessageHelper
{
    public static string GetId(string userId, string messageType)
    {
        if (userId.IsNullOrEmpty() || messageType.IsNullOrEmpty())
        {
            throw new UserFriendlyException("invalid params");
        }

        return $"{userId}-{messageType}";
    }

    public static int GetUnreadCount(List<UnreadMessageInfo> unreadMessageInfos)
    {
        if (unreadMessageInfos.IsNullOrEmpty()) return 0;

        return unreadMessageInfos.Select(t => t.UnreadCount).Sum();
    }

    public static int GetUnreadCount(List<UnreadMessageInfo> unreadMessageInfos, MessageType messageType)
    {
        if (unreadMessageInfos.IsNullOrEmpty()) return 0;

        var count = unreadMessageInfos
            .FirstOrDefault(t => t.MessageType.Equals(messageType.ToString(), StringComparison.OrdinalIgnoreCase))
            ?.UnreadCount;

        return count ?? 0;
    }
}