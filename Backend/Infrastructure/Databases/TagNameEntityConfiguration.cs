using Application.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Constants;

namespace Infrastructure.Databases;

public class TagNameEntityConfiguration : IEntityTypeConfiguration<TagNameEntity>
{
    public void Configure(EntityTypeBuilder<TagNameEntity> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.ModifiedBy);
        builder.Property(u => u.Status).IsRequired().IsUnicode().HasMaxLength(Validator.FieldLength.SingleLineOfText);

        builder.ToTable(Constants.TableName.TagName);
    }
}