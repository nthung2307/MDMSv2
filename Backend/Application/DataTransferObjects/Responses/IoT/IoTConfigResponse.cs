using System;

namespace Application.DataTransferObjects.Responses.IoT;

public class IoTConfigResponse
{
    public string ApiKey { get; set; } = string.Empty;
    public string AllowedIps { get; set; } = string.Empty;
    public bool RequireApiKey { get; set; } = false;
    public DateTimeOffset? LastPushTime { get; set; }
    public string? LastPushIp { get; set; }
    public string? LastPushStatus { get; set; }
    public int LastPushTagCount { get; set; }
    public long TotalPushCount { get; set; }
    public bool IsActiveRecently { get; set; }
}

