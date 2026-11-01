using Application.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobisoft.Core.Constants;

namespace Infrastructure.Databases;

public class PeriodicEnergyStatementEntityConfiguration : IEntityTypeConfiguration<PeriodicEnergyStatementEntity>
{
    public void Configure(EntityTypeBuilder<PeriodicEnergyStatementEntity> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();

        builder.HasOne(r => r.MeterReplacement)
            .WithMany()
            .HasForeignKey(r => r.MeterReplacementId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.StatementPdfTemplate)
            .WithMany()
            .HasForeignKey(r => r.StatementPdfTemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable(Constants.TableName.PeriodicEnergyStatement);
    }
}