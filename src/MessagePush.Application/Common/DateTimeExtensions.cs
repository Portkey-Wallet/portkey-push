using System;

namespace MessagePush.Common;

public static class DateTimeExtensions
{
    public static DateTime SubtractDays(this DateTime date, int days)
    {
        return date.AddDays(-days);
    }
}