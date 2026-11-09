using Application.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobisoft.Core.Constants;

namespace Infrastructure.Databases;

public class MeasurementPointsEntityConfiguration : IEntityTypeConfiguration<MeasurementPointsEntity>
{
    public void Configure(EntityTypeBuilder<MeasurementPointsEntity> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();
        builder.Property(r => r.MultiplierFactor).HasPrecision(18, 4).HasDefaultValue(1.0m);
        builder.HasIndex(r => r.Code);
        builder.HasIndex(r => r.ModifiedBy);
        builder.HasOne(m => m.Customer)
            .WithMany(c => c.MeasurePoints)
            .HasForeignKey(m => m.CustomerId);

        builder.ToTable(Constants.TableName.MeasurementPoints);
    }
}