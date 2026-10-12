using Application.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Constants;

namespace Infrastructure.Databases;

public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.ModifiedBy);
        builder.Property(u => u.Status).IsRequired().IsUnicode().HasMaxLength(Validator.FieldLength.SingleLineOfText);

        builder.ToTable(Constants.TableName.Users);

        var records = new List<UserEntity>();
        var accountSystem = new UserEntity()
        {
            Id = 921946681335811,
            UserName = "admin",
            Password = "AM32iAMOaG2CYyWsW/9HqGZBQoLiiU97kZcGQmby63M6af+59perqJu22v9YaiqajA==", //Abc@123456
            RoleName = DefaultRoles.SuperAdmin,
            FullName = "Super Admin",
            Email = "admin@gmail.com",
            PhoneNumber = "0966666666",
            Address = "Hà Nội",
            Status = EntityStatus.Active,
            CreatedBy = 921946681335812,
            ModifiedBy = 921946681335812,
            ModifiedTime = DateTime.UtcNow,
            CreatedTime = DateTime.UtcNow,
            Deleted = false
        };
        records.Add(accountSystem);
        builder.HasData(records);
    }
}