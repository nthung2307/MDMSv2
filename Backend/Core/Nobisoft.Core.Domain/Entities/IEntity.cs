namespace Nobisoft.Core.Domain.Entities
{
    /// <summary>
    /// The base class for an Entity
    /// </summary>
    public interface IEntity
    {
    }

    /// <summary>
    /// The generic base class for an Entity. Each Entity must have Id field
    /// </summary>
    /// <typeparam name="TKey">Data type of ID</typeparam>
    public interface IEntity<TKey> : IEntity
    {
        TKey Id { get; }
    }
}