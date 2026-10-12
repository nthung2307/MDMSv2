using Application.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobisoft.Core.Constants;

namespace Infrastructure.Databases;

public class GcsEntityConfiguration : IEntityTypeConfiguration<GcsEntity>
{
    public void Configure(EntityTypeBuilder<GcsEntity> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();
        builder.HasIndex(r => r.ModifiedBy);
        builder.Property(e => e.DayPeriod)
            .HasColumnType("json");

        builder.ToTable(Constants.TableName.Gcs);
    }
}