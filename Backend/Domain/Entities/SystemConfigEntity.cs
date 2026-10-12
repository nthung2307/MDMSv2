using Nobisoft.Core.Domain.Entities;

namespace Domain.Entities;

public class SystemConfigEntity : IEntity<long>
{
    public long Id { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
}