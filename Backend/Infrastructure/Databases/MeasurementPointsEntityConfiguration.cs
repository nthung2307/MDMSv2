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
        builder.Property(r => r.MeterSerial).HasMaxLength(100);
        builder.Property(r => r.ApiKeyHash).HasMaxLength(128);
        builder.Property(r => r.ApiKeyEncrypted).HasMaxLength(512);
        builder.Property(r => r.KeyPrefix).HasMaxLength(32);
        builder.Property(r => r.MappingMode).HasMaxLength(20).HasDefaultValue("HYBRID");
        builder.HasIndex(r => r.Code);
        builder.HasIndex(r => r.MeterSerial);
        builder.HasIndex(r => r.ModifiedBy);
        builder.HasOne(m => m.Customer)
            .WithMany(c => c.MeasurePoints)
            .HasForeignKey(m => m.CustomerId);
        builder.HasOne(m => m.MeterProfile)
            .WithMany(p => p.MeasurementPoints)
            .HasForeignKey(m => m.MeterProfileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable(Constants.TableName.MeasurementPoints);
    }
}