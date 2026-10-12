using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Nobisoft.Core.Domain.Interfaces;
using Nobisoft.Core.Domain.ValueObjects;

namespace Nobisoft.Core.Domain.AggregateRoots
{
    /// <summary>
    /// Full aggregate root with concurrency timestamp and extra properties
    /// </summary>
    public class FullAggregateRoot : FullAuditAggregateRoot, IHasConcurrencyTimestamp, IHasExtraProperties
    {
        public ICollection<ExtraProperty> ExtraProperties { get; set; }

        public uint ConcurrencyStamp { get; set; }
    }

    /// <summary>
    /// Full aggregate root with concurrency timestamp and extra properties
    /// </summary>
    /// <typeparam name="TKey">Data type of ID</typeparam>
    public class FullAggregateRoot<TKey> : FullAggregateRoot, IAggregateRoot<TKey>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual TKey Id { get; protected set; }

        public virtual TKey TenantId { get; protected set; }
    }
}