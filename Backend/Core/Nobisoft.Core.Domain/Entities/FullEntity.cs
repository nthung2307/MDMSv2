using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Nobisoft.Core.Domain.Interfaces;
using Nobisoft.Core.Domain.ValueObjects;

namespace Nobisoft.Core.Domain.Entities
{
    /// <summary>
    /// Full entity with audit, concurrency and extra properties
    /// </summary>
    public class FullEntity : FullAuditEntity, IHasConcurrencyTimestamp, IHasExtraProperties
    {
        public uint ConcurrencyStamp { get; set; }
        public ICollection<ExtraProperty> ExtraProperties { get; set; }
    }

    /// <summary>
    /// Full entity with audit, concurrency and extra properties
    /// </summary>
    /// <typeparam name="TKey">Data type of ID</typeparam>
    public class FullEntity<TKey> : FullAuditEntity<TKey>, IEntity<TKey>
    {
        /// <summary>
        /// This column will be hidden once we use Postgresql
        /// </summary>
        public uint ConcurrencyStamp { get; set; }
        public ICollection<ExtraProperty> ExtraProperties { get; set; } = new List<ExtraProperty>();
    }
}