namespace Nobisoft.Core.Domain.Entities
{
    /// <summary>
    /// The base class for an Entity
    /// </summary>
    public abstract class Entity: IEntity
    {
        
    }
    public abstract class Entity<TKey>: Entity
    {
        public virtual TKey Id { get;  set; }

        public virtual TKey TenantId { get;  set; }
    }
}