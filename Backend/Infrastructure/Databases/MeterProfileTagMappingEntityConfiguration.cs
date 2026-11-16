using Application.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Databases;

public class MeterProfileTagMappingEntityConfiguration : IEntityTypeConfiguration<MeterProfileTagMappingEntity>
{
    public void Configure(EntityTypeBuilder<MeterProfileTagMappingEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.ObisCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ParameterCode).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Unit).HasMaxLength(32);
        builder.Property(x => x.CustomName).HasMaxLength(255);
        builder.Property(x => x.LegacyAliases).HasMaxLength(1000);
        builder.Property(x => x.DataCategory).HasMaxLength(64).HasDefaultValue("ELECTRICAL");

        builder.HasIndex(x => new { x.MeterProfileId, x.ObisCode });
        builder.HasIndex(x => new { x.MeterProfileId, x.ParameterCode });

        builder.HasOne(x => x.MeterProfile)
            .WithMany(p => p.TagMappings)
            .HasForeignKey(x => x.MeterProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(Constants.TableName.MeterProfileTagMappings);
    }
}

