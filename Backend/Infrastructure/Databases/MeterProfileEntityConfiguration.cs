using Application.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Databases;

public class MeterProfileEntityConfiguration : IEntityTypeConfiguration<MeterProfileEntity>
{
    public void Configure(EntityTypeBuilder<MeterProfileEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.VoltageLevel).HasMaxLength(32).HasDefaultValue("LV");
        builder.Property(x => x.DefaultVoltageUnit).HasMaxLength(16).HasDefaultValue("V");
        builder.Property(x => x.DefaultPowerUnit).HasMaxLength(16).HasDefaultValue("kW");
        builder.HasIndex(x => x.Code).IsUnique();
        builder.ToTable(Constants.TableName.MeterProfiles);
    }
}

