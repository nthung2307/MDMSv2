using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Nobisoft.Core.Domain.Interfaces.Audit;

namespace Nobisoft.Core.Domain.Entities
{
    /// <summary>
    /// Full Audited entity
    /// </summary>
    public class FullAuditEntity : Entity, IAuditEntity
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
    /// Full Audited entity
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class FullAuditEntity<TKey> : Entity<TKey>, IAuditEntity<TKey>
    {
        

        public DateTimeOffset CreatedTime { get; set; }
        public long? CreatedBy { get; set; }
        public DateTimeOffset ModifiedTime { get; set; }
        public long? ModifiedBy { get; set; }
        public bool Deleted { get; set; }
        public long? DeletedBy { get; set; }
        public DateTimeOffset DeletedTime { get; set; }
    }
}