using Application.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Databases;

public class MeasurementPointDailyEnergyReportConfiguration : IEntityTypeConfiguration<MeasurementPointDailyEnergyReportEntity>
{
    public void Configure(EntityTypeBuilder<MeasurementPointDailyEnergyReportEntity> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();
        builder.HasIndex(x => x.CreatedTime);
        builder.HasIndex(x => x.MeasurementPointId);
        builder.HasIndex(x => new { x.MeasurementPointId, x.CreatedTime }).IsUnique();

        builder.Property(x => x.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoTichLuyDienNangTacDungAPTongKenhGiao).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoTichLuyDienNangTacDungAPTongKenhNhan).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan).HasPrecision(18, 4);

        builder.ToTable(Constants.TableName.MeasurementPointDailyEnergyReport);
    }
}
