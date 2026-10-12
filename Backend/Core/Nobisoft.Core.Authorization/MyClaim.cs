namespace Nobisoft.Core.Authorization;

public class MyClaim
{
    public MyClaim()
    {
        
    }

    public MyClaim(string type, string value, string valueType)
    {
        Type = type;
        Value = value;
        ValueType = valueType;
    }

    public string Type { get; set; }
    public string Value { get; set; }
    public string ValueType { get; set; }
}