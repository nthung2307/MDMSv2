using Nobisoft.Core.Constants;
using Nobisoft.Core.Domain.Entities;

namespace Domain.Entities;

public class CustomerEntity : IEntity<long>
{
    public long Id { get; set; }
    public string? CustomerCode { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string Status { get; set; } = EntityStatus.Deactivate;
    public long? CreatedBy { get; set; }
    public string? CreatedName { get; set; }
    public DateTimeOffset? CreatedTime { get; set; }
    public DateTimeOffset? ModifiedTime { get; set; }
    public long? ModifiedBy { get; set; }
    public string? ModifiedName { get; set; }
    public bool Deleted { get; set; }
    public DateTimeOffset? DeletedTime { get; set; }
    public long? DeletedBy { get; set; }
    public string? DeletedName { get; set; }
    public List<MeasurementPointsEntity> MeasurePoints { get; set; } = new List<MeasurementPointsEntity>();
    public List<UserCustomerEntity> UserCustomer { get; set; } = new List<UserCustomerEntity>();
}