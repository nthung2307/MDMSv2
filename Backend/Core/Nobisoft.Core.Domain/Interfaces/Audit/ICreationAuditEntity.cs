using System;

namespace Nobisoft.Core.Domain.Interfaces.Audit
{
    /// <summary>
    /// Track creation action
    /// </summary>
    public interface ICreationAuditEntity
    {
        DateTimeOffset CreatedTime { get; }

        long? CreatedBy { get; }
        
    }
}