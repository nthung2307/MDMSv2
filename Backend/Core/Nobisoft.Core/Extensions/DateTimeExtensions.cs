using System;

namespace Nobisoft.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTimeOffset Truncate(this DateTimeOffset dateTimeOffset, TimeSpan timeSpan)
        {
            return timeSpan == TimeSpan.Zero || dateTimeOffset == DateTimeOffset.MinValue || dateTimeOffset == DateTimeOffset.MaxValue ? dateTimeOffset : dateTimeOffset.AddTicks(-(dateTimeOffset.Ticks % timeSpan.Ticks));
        }

        public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
        {
            return timeSpan == TimeSpan.Zero || dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue ? dateTime : dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }
    }
}