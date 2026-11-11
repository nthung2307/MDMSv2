using Application.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Databases;

public class MeasurementPointEventEntityConfiguration : IEntityTypeConfiguration<MeasurementPointsEventEntity>
{
    public void Configure(EntityTypeBuilder<MeasurementPointsEventEntity> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();
        builder.HasIndex(x => x.CreatedTime);
        builder.HasIndex(x => new { x.MeasurementPointId, x.CreatedTime });
        builder.Property(x => x.ParameterCode).HasMaxLength(64);

        builder.HasOne(x => x.TagName)
            .WithMany()
            .HasForeignKey(x => x.TagNameId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable(Constants.TableName.MeasurementPointEvents);
    }
}
