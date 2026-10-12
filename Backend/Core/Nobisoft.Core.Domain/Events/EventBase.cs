using MediatR;
using Nobisoft.Core.ValueObjects;

namespace Nobisoft.Core.Domain.Events
{
    /// <summary>
    /// The event base for all domain events
    /// </summary>
    public class EventBase : INotification
    {
    }

    /// <summary>
    /// The event base for all domain events
    /// </summary>
    /// <typeparam name="TKey">Data type of ID</typeparam>
    public class EventBase<TKey> : EventBase
    {
        /// <summary>
        /// Aggregate Id
        /// </summary>
        public TKey Id { get; private set; }

        public TKey TenantId { get; private set; }
        public TKey TransactionId { get; set; }
        public TKey CreatedBy { get; set; }
        public Tracker Tracker { get; set; }
        public TKey Revision { get; set; }

        public EventBase(TKey id, TKey tenantId, TKey transactionId, TKey revision, TKey createdBy, Tracker tracker)
        {
            Id = id;
            TenantId = tenantId;
            TransactionId = transactionId;
            Tracker = tracker;
            CreatedBy = createdBy;
            Revision = revision;
        }

        public EventBase(TKey id, TKey transactionId, TKey revision, TKey createdBy, Tracker tracker)
        {
            Id = id;
            TransactionId = transactionId;
            Tracker = tracker;
            CreatedBy = createdBy;
            Revision = revision;
        }
    }
}