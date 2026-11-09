using Application.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobisoft.Core.Constants;

namespace Infrastructure.Databases;

public class GcsTemporaryEntityConfiguration : IEntityTypeConfiguration<GcsTemporaryEntity>
{
    public void Configure(EntityTypeBuilder<GcsTemporaryEntity> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();
        builder.Property(r => r.PreviousReading).HasPrecision(18, 4);
        builder.Property(r => r.CurrentReading).HasPrecision(18, 4);
        builder.Property(r => r.NewCurrentReading).HasPrecision(18, 4);
        builder.Property(r => r.NewPreviousReading).HasPrecision(18, 4);
        builder.Property(r => r.MultiplierFactor).HasPrecision(18, 4).HasDefaultValue(1.0m);
        builder.Property(r => r.Consumption).HasPrecision(18, 4);
        builder.Property(r => r.CosPhi).HasPrecision(5, 2);
        builder.Property(r => r.IsSuspicious).HasDefaultValue(false);
        builder.Property(r => r.SuspiciousReason).HasMaxLength(500);
        builder.Property(r => r.IsChange).HasDefaultValue(false);

        builder.ToTable(Constants.TableName.GcsTemporary);
    }
}