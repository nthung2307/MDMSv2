using System;

namespace Nobisoft.Core.Domain.Interfaces.Audit
{
    /// <summary>
    /// Track modification action
    /// </summary>
    public interface IModificationAuditEntity
    {
        DateTimeOffset ModifiedTime { get; }

        long? ModifiedBy { get; }
    }
}