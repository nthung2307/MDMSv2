using Application.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Databases;

public class SystemEmailConfigEntityConfiguration : IEntityTypeConfiguration<SystemEmailConfigEntity>
{
    public void Configure(EntityTypeBuilder<SystemEmailConfigEntity> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.ProviderType).IsRequired().HasMaxLength(50);
        builder.Property(r => r.SenderEmail).IsRequired().HasMaxLength(255);
        builder.Property(r => r.SenderDisplayName).HasMaxLength(255);
        
        builder.Property(r => r.TenantId).HasMaxLength(100);
        builder.Property(r => r.ClientId).HasMaxLength(100);
        builder.Property(r => r.EncryptedClientSecret).HasMaxLength(1000);
        
        builder.Property(r => r.SmtpHost).HasMaxLength(255);
        builder.Property(r => r.SecurityMode).HasMaxLength(50);
        builder.Property(r => r.SmtpUsername).HasMaxLength(255);
        builder.Property(r => r.EncryptedSmtpPassword).HasMaxLength(1000);

        builder.ToTable(Constants.TableName.SystemEmailConfigs);
    }
}
