using Application.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobisoft.Core.Constants;

namespace Infrastructure.Databases;

public class ArchiveDataMeasurementPointMonitoringEntityConfiguration : IEntityTypeConfiguration<ArchiveDataMeasurementPointMonitoringEntity>
{
    public void Configure(EntityTypeBuilder<ArchiveDataMeasurementPointMonitoringEntity> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();

        builder.ToTable(Constants.TableName.ArchiveDataMeasurementPointMonitoring);
    }
}