using Application.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobisoft.Core.Constants;

namespace Infrastructure.Databases;

public class GcsCutoffCycleEntityConfiguration : IEntityTypeConfiguration<GcsCutoffCycleEntity>
{
    public void Configure(EntityTypeBuilder<GcsCutoffCycleEntity> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();

        builder.ToTable(Constants.TableName.GcsCutoffCycle);
    }
}