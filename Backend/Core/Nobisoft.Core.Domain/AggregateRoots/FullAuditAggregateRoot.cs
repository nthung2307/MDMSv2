using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Nobisoft.Core.Domain.Interfaces.Audit;

namespace Nobisoft.Core.Domain.AggregateRoots
{
    
    /// <summary>
    /// Full aggregate root with the audit info
    /// </summary>
    public class FullAuditAggregateRoot : AggregateRoot, IAuditEntity
    {
        public DateTimeOffset CreatedTime { get; set; }

        public long? CreatedBy { get; set; }

        public DateTimeOffset ModifiedTime { get; set; }

        public long? ModifiedBy { get; set; }

        public bool Deleted { get; set; }

        public long? DeletedBy { get; set; }

        public DateTimeOffset DeletedTime { get; set; }
    }

    /// <summary>
    /// Full aggregate root with the audit info and the Id
    /// </summary>
    /// <typeparam name="TKey">Data type of ID</typeparam>
    public class FullAuditAggregateRoot<TKey> : FullAuditAggregateRoot, IAggregateRoot<TKey>, IAuditEntity<TKey>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual TKey Id { get; protected set; }
        public virtual TKey TenantId { get; protected set; }
    }
}