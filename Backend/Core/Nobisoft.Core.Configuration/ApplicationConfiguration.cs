namespace Nobisoft.Core.Configuration;

public class ApplicationConfiguration
{
    public int LockoutDurationInMin { get; set; }
    public int ReminderPeriod { get; set; }
    public string ApiKeyHeader { get; set; }
    public string ApiKeyValue { get; set; }
    public string NotificationDomain { get; set; }
    public string NotificationTopic { get; set; }
    public string BasicAuthUserName { get; set; }
    public string BasicAuthPassword { get; set; }
}