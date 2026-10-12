using Application.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobisoft.Core.Constants;

namespace Infrastructure.Databases;

public class PeriodicEnergyStatementEntityConfiguration : IEntityTypeConfiguration<PeriodicEnergyStatementEntity>
{
    public void Configure(EntityTypeBuilder<PeriodicEnergyStatementEntity> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();
        builder.ToTable(Constants.TableName.PeriodicEnergyStatement);
    }
}