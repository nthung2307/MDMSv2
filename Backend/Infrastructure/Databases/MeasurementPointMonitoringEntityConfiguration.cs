using Application.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobisoft.Core.Constants;

namespace Infrastructure.Databases;

public class MeasurementPointMonitoringEntityConfiguration : IEntityTypeConfiguration<MeasurementPointMonitoringEntity>
{
    public void Configure(EntityTypeBuilder<MeasurementPointMonitoringEntity> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();
        builder.HasIndex(x => x.CreatedTime);


        builder.ToTable(Constants.TableName.MeasurementPointMonitoring);
    }
}