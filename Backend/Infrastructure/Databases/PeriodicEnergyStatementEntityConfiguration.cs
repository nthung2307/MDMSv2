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

        builder.Property(r => r.Bt).HasPrecision(18, 4);
        builder.Property(r => r.Cd).HasPrecision(18, 4);
        builder.Property(r => r.Td).HasPrecision(18, 4);
        builder.Property(r => r.Sg).HasPrecision(18, 4);
        builder.Property(r => r.Vc).HasPrecision(18, 4);
        builder.Property(r => r.Pmax).HasPrecision(18, 4);
        builder.Property(r => r.PreviousBt).HasPrecision(18, 4);
        builder.Property(r => r.PreviousCd).HasPrecision(18, 4);
        builder.Property(r => r.PreviousTd).HasPrecision(18, 4);
        builder.Property(r => r.PreviousSg).HasPrecision(18, 4);
        builder.Property(r => r.PreviousVc).HasPrecision(18, 4);
        builder.Property(r => r.PreviousPmax).HasPrecision(18, 4);
        builder.Property(r => r.ConsumtionBt).HasPrecision(18, 4);
        builder.Property(r => r.ConsumtionCd).HasPrecision(18, 4);
        builder.Property(r => r.ConsumtionTd).HasPrecision(18, 4);
        builder.Property(r => r.ConsumtionSg).HasPrecision(18, 4);
        builder.Property(r => r.ConsumtionVc).HasPrecision(18, 4);
        builder.Property(r => r.Multiplier).HasPrecision(18, 4).HasDefaultValue(1.0m);
        builder.Property(r => r.CosPhi).HasPrecision(5, 2);

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