using Nobisoft.Core.Domain.Entities;

namespace Nobisoft.Core.Domain.Interfaces.Audit
{
    public interface IAuditEntity: ICreationAuditEntity, IModificationAuditEntity, IDeletionAuditEntity, IEntity
    {
        
    }
    public interface IAuditEntity<TKey>: ICreationAuditEntity, IModificationAuditEntity, IDeletionAuditEntity, IEntity<TKey>
    {
        
    }

}