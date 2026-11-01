using Application.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Databases;

public class NotificationTemplateEntityConfiguration : IEntityTypeConfiguration<NotificationTemplateEntity>
{
    public void Configure(EntityTypeBuilder<NotificationTemplateEntity> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.Code).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(255);
        builder.Property(r => r.SubjectTemplate).IsRequired().HasMaxLength(500);
        builder.Property(r => r.BodyHtmlTemplate).IsRequired().HasColumnType("LONGTEXT");
        builder.Property(r => r.Description).HasMaxLength(1000);

        builder.HasIndex(r => r.Code);

        builder.ToTable(Constants.TableName.NotificationTemplates);
    }
}
