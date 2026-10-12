using Application.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Constants;

namespace Infrastructure.Databases;

public class UserCustomerEntityConfiguration : IEntityTypeConfiguration<UserCustomerEntity>
{
    public void Configure(EntityTypeBuilder<UserCustomerEntity> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();

        builder.ToTable(Constants.TableName.UserCustomers);
    }
}