using Nobisoft.Core.Domain.Entities;

namespace Domain.Entities;

public class UserCustomerEntity : IEntity<long>
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public CustomerEntity Customer { get; set; }
    public long UserId { get; set; }
    public UserEntity User { get; set; }
}