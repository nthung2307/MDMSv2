using Application.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Databases;

public class StatementPdfTemplateEntityConfiguration : IEntityTypeConfiguration<StatementPdfTemplateEntity>
{
    public void Configure(EntityTypeBuilder<StatementPdfTemplateEntity> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.Code).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(255);
        builder.Property(r => r.HeaderTitle).HasMaxLength(255);
        builder.Property(r => r.SubHeaderTitle).HasMaxLength(255);
        builder.Property(r => r.CompanyName).HasMaxLength(255);
        builder.Property(r => r.CompanyAddress).HasMaxLength(500);
        builder.Property(r => r.ContactPhone).HasMaxLength(255);
        builder.Property(r => r.LogoBase64).HasColumnType("LONGTEXT");
        builder.Property(r => r.PaperSize).HasMaxLength(50);
        builder.Property(r => r.DisplayColumnsJson).HasMaxLength(1000);
        builder.Property(r => r.FooterNotes).HasMaxLength(2000);

        builder.HasIndex(r => r.Code);

        builder.ToTable(Constants.TableName.StatementPdfTemplates);
    }
}
