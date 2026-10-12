using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MediatR;
using Nobisoft.Core.Domain.Events;
using Nobisoft.Core.Domain.ValueObjects;

namespace Nobisoft.Core.Domain.AggregateRoots
{
    /// <summary>
    ///  Provide base methods for an aggregate root without id related to domain event
    /// </summary>
    public class AggregateRoot : IAggregateRoot
    {
        
        private List<EventBase<long>> _domainEvents = new();

        /// <summary>
        /// Get list domain events of the aggregate root
        /// </summary>
        public IReadOnlyCollection<EventBase<long>> DomainEvents
        {
            get
            {
                var domainEvents = _domainEvents;
                return domainEvents.AsReadOnly();
            }
        }

        /// <summary>
        /// Add an event into the aggregate root's domain events
        /// </summary>
        /// <param name="event"></param>
        public void AddDomainEvent(EventBase<long> @event)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            _domainEvents ??= new List<EventBase<long>>();
            _domainEvents.Add(@event);
        }

        /// <summary>
        /// Remove an event from the aggregate root's domain events
        /// </summary>
        /// <param name="event"></param>
        public void RemoveDomainEvent(EventBase<long> @event)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            _domainEvents ??= new List<EventBase<long>>();
            _domainEvents.Remove(@event);
        }

        /// <summary>
        /// Remove all events in the aggregate root's domain events
        /// </summary>
        public void ClearDomainEvents()
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            _domainEvents ??= new List<EventBase<long>>();
            _domainEvents.Clear();
        }
        public List<EventHistoryItem<long>> EventHistories { get; set; } = new List<EventHistoryItem<long>>();
    }

    /// <summary>
    /// Provide base methods for an aggregate root with id related to domain event
    /// </summary>
    /// <typeparam name="TKey">Data type of the ID key</typeparam>
    public class AggregateRoot<TKey> : AggregateRoot, IAggregateRoot<TKey>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual TKey Id { get; protected set; }
        
        //public List<EventHistoryItem<TKey>> EventHistories { get; set; } = new List<EventHistoryItem<TKey>>();
    }
}