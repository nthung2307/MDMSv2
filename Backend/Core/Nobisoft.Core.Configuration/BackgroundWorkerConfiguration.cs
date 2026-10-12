using System.Diagnostics.CodeAnalysis;

#pragma warning disable 1591
namespace Nobisoft.Core.Configuration
{
    [ExcludeFromCodeCoverage]
    public class BackgroundWorkerConfiguration
    {
        /// <summary>
        /// Buffer time to make sure worker can schedule a task before passing the planned time, should be 15mins before
        /// </summary>
        public int BufferTimeInSecondBeforeSchedulingATask { get; set; } = 0;

        /// <summary>
        /// Buffer time to make sure worker can cancel a task before sending as scheduled, should be 10mins before the scheduled time
        /// </summary>
        public int BufferTimeInSecondBeforeCancellingAScheduledTask { get; set; } = 0;
        // To define cron expression
        public string CronExpression { get; set; } = "";

        // https://en.wikipedia.org/wiki/Cron#CRON_expression
        // To define timezone
        public string TimeZone { get; set; } = "";

        // To define task name
        public string TaskName { get; set; } = "";
        public int BatchSize { get; set; } = 10000;
    }
}