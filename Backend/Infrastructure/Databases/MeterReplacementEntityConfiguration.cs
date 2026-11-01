using Application.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Databases;

public class MeterReplacementEntityConfiguration : IEntityTypeConfiguration<MeterReplacementEntity>
{
    public void Configure(EntityTypeBuilder<MeterReplacementEntity> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.Reason).HasMaxLength(500);
        builder.Property(r => r.OldMeterSerial).IsRequired().HasMaxLength(100);
        builder.Property(r => r.NewMeterSerial).IsRequired().HasMaxLength(100);
        builder.Property(r => r.DocumentUrl).HasMaxLength(1000);
        builder.Property(r => r.Status).HasMaxLength(50);
        builder.Property(r => r.CreatedName).HasMaxLength(255);

        builder.HasOne(r => r.MeasurementPoint)
            .WithMany()
            .HasForeignKey(r => r.MeasurementPointId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.MeasurementPointId);
        builder.HasIndex(r => r.ReplacementDate);

        builder.ToTable(Constants.TableName.MeterReplacements);
    }
}
