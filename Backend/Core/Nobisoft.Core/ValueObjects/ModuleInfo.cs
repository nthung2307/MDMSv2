namespace Nobisoft.Core.ValueObjects;

/// <summary>
/// Hold the module info
/// </summary>
public class ModuleInfo
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string MinimumCoreVersion { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? ChangeLog { get; set; }
    public string? Website { get; set; }
    public string? Icon { get; set; } = string.Empty;
    public long CategoryId { get; set; }
    public string License { get; set; } = string.Empty;
    public ICollection<string> Scopes { get; set; } = new List<string>();
    public int Order { get; set; } = 1;
    public bool AutoInstall { get; set; } = false;
    public bool ToBuy { get; set; } = false;
}