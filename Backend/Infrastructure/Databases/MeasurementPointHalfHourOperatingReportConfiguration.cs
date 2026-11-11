using Application.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Databases;

public class MeasurementPointHalfHourOperatingReportConfiguration : IEntityTypeConfiguration<MeasurementPointHalfHourOperatingReportEntity>
{
    public void Configure(EntityTypeBuilder<MeasurementPointHalfHourOperatingReportEntity> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.HasIndex(x => x.CreatedTime);
        builder.HasIndex(x => x.MeasurementPointId);
        builder.HasIndex(x => new { x.MeasurementPointId, x.CreatedTime }).IsUnique();

        builder.HasOne(x => x.MeasurementPoint)
            .WithMany()
            .HasForeignKey(x => x.MeasurementPointId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(Constants.TableName.MeasurementPointHalfHourOperatingReport);
    }
}
