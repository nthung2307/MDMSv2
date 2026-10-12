namespace Application.DataTransferObjects.Requests.IoT;

public class IoTDataRequest
{
    public string? Meter { get; set; }
    public long Timestamp { get; set; }
    public List<IoTDataValue> Values { get; set; }
}

public class IoTDataValue
{
    public string Id { get; set; }
    public dynamic V { get; set; }
    public string Q { get; set; }
    public long T { get; set; }
}