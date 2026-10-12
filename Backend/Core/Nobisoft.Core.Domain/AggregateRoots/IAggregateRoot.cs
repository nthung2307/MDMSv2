using MediatR;
using Nobisoft.Core.Domain.Entities;
using Nobisoft.Core.Domain.Events;

namespace Nobisoft.Core.Domain.AggregateRoots
{
    /// <summary>
    /// Provide base methods for an aggregate root without id related to domain event
    /// </summary>
    public interface IAggregateRoot : IEntity
    {
        IReadOnlyCollection<EventBase<long>> DomainEvents { get; }
        void AddDomainEvent(EventBase<long> @event);
        void RemoveDomainEvent(EventBase<long> @event);
        void ClearDomainEvents();
    }
    /// <summary>
    /// Provide base methods for an aggregate root with id related to domain event.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IAggregateRoot<TKey> : IAggregateRoot, IEntity<TKey>
    {
        
    }
}