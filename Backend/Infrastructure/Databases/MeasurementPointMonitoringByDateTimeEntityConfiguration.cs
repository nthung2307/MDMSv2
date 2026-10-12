using Application.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobisoft.Core.Constants;

namespace Infrastructure.Databases;

public class MeasurementPointMonitoringByDateTimeEntityConfiguration : IEntityTypeConfiguration<MeasurementPointMonitoringByDateTimeEntity>
{
    public void Configure(EntityTypeBuilder<MeasurementPointMonitoringByDateTimeEntity> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();
        builder.HasIndex(x => x.CreatedTime);


        builder.ToTable(Constants.TableName.MeasurementPointMonitoringByDateTime);
    }
}