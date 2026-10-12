namespace Application.DataTransferObjects.Requests.SystemConfig;

public class CreateUpdateSystemConfigRequest
{
    public List<ConfigRequest> Configs { get; set; }
}

public class ConfigRequest
{
    public required string Key { get; set; }
    public required string Value { get; set; }
}