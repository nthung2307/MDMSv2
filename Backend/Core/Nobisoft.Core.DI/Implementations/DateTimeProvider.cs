using System;
using Nobisoft.Core.DI.Interfaces;

namespace Nobisoft.Core.DI.Implementations
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset Now => DateTimeOffset.Now;
        public DateTimeOffset NowUtc => DateTimeOffset.UtcNow;

        public DateTimeOffset NowWithOffset(TimeSpan offset)
        {
            var now = DateTime.Now;
            return new DateTimeOffset(now.Year,now.Month, now.Day,now.Hour,
                now.Minute,now.Second,now.Millisecond, offset);
        }
    }
}