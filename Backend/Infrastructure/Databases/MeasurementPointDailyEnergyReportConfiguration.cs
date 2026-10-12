using Application.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Databases;

public class MeasurementPointDailyEnergyReportConfiguration : IEntityTypeConfiguration<MeasurementPointDailyEnergyReportEntity>
{
    public void Configure(EntityTypeBuilder<MeasurementPointDailyEnergyReportEntity> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();
        builder.HasIndex(x => x.CreatedTime);
        builder.HasIndex(x => x.MeasurementPointId);

        builder.ToTable(Constants.TableName.MeasurementPointDailyEnergyReport);
    }
}