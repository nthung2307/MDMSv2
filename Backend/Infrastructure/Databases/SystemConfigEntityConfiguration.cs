using Application.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Databases;

public class SystemConfigEntityConfiguration : IEntityTypeConfiguration<SystemConfigEntity>
{
    public void Configure(EntityTypeBuilder<SystemConfigEntity> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();

        builder.ToTable(Constants.TableName.SystemConfig);
    }
}