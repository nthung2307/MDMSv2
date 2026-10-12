using System;

namespace Nobisoft.Core.Domain.Interfaces.Audit
{
    /// <summary>
    /// Track deletion action
    /// </summary>
    public interface IDeletionAuditEntity
    {
        bool Deleted { get; }

        long? DeletedBy { get; }

        DateTimeOffset DeletedTime { get; }
    }
}