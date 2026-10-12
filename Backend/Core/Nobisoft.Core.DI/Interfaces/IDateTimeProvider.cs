using System;
using Nobisoft.Core.Lifetimes;

namespace Nobisoft.Core.DI.Interfaces
{
    public interface IDateTimeProvider : IScopedDependency
    {
        /// <summary>
        /// Get now with server timezone
        /// </summary>
        public DateTimeOffset Now { get; }

        /// <summary>
        /// Get now with UTC timezone
        /// </summary>
        public DateTimeOffset NowUtc { get; }

        /// <summary>
        /// Get now with the given timezone
        /// </summary>
        /// <param name="offset">Timezone under timespan</param>
        /// <returns></returns>
        public DateTimeOffset NowWithOffset(TimeSpan offset);
    }
}