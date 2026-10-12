namespace Nobisoft.Core.ValueObjects;

public class PropertyInfo
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string? Value { get; set; }
}