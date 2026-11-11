using Application.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Databases;

public class MeasurementPointCurrentSnapshotEntityConfiguration : IEntityTypeConfiguration<MeasurementPointCurrentSnapshotEntity>
{
    public void Configure(EntityTypeBuilder<MeasurementPointCurrentSnapshotEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.HasIndex(x => x.MeasurementPointId).IsUnique();

        builder.Property(x => x.DongDienPhaA).HasPrecision(18, 4);
        builder.Property(x => x.DongDienPhaB).HasPrecision(18, 4);
        builder.Property(x => x.DongDienPhaC).HasPrecision(18, 4);
        builder.Property(x => x.DienApPhaA).HasPrecision(18, 4);
        builder.Property(x => x.DienApPhaB).HasPrecision(18, 4);
        builder.Property(x => x.DienApPhaC).HasPrecision(18, 4);
        builder.Property(x => x.CongSuatTacDungPhaA).HasPrecision(18, 4);
        builder.Property(x => x.CongSuatTacDungPhaB).HasPrecision(18, 4);
        builder.Property(x => x.CongSuatTacDungPhaC).HasPrecision(18, 4);
        builder.Property(x => x.CongSuatTacDungTong3Pha).HasPrecision(18, 4);
        builder.Property(x => x.CongSuatPhanKhangPhaA).HasPrecision(18, 4);
        builder.Property(x => x.CongSuatPhanKhangPhaB).HasPrecision(18, 4);
        builder.Property(x => x.CongSuatPhanKhangPhaC).HasPrecision(18, 4);
        builder.Property(x => x.CongSuatPhanKhangTong3Pha).HasPrecision(18, 4);
        builder.Property(x => x.HeSoCongSuatPFPhaA).HasPrecision(18, 4);
        builder.Property(x => x.HeSoCongSuatPFPhaB).HasPrecision(18, 4);
        builder.Property(x => x.HeSoCongSuatPFPhaC).HasPrecision(18, 4);
        builder.Property(x => x.HeSoCongSuatPFTrungBinh3Pha).HasPrecision(18, 4);
        builder.Property(x => x.TanSoHeThong).HasPrecision(18, 4);

        builder.Property(x => x.ChiSoTichLuyDienNangTacDungAPBieuGioBT).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoTichLuyDienNangTacDungAPBieuGioCD).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoTichLuyDienNangTacDungAPBieuGioTD).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoTichLuyDienNangTacDungAPTong).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoTichLuyDienNangPhanKhangAQTong).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoTichLuyDienNangTacDungAPTongChieuNhan).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan).HasPrecision(18, 4);

        builder.Property(x => x.ChiSoChotDienNangAPGioBT).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoChotDienNangAPGioCD).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoChotDienNangAPGioTD).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoChotDienNangAPTong).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoChotDienNangAQTong).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoChotDienNangAPGioBTChieuNhan).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoChotDienNangAPGioCDChieuNhan).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoChotDienNangAPGioTDChieuNhan).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoChotDienNangAPTongChieuNhan).HasPrecision(18, 4);
        builder.Property(x => x.ChiSoChotDienNangAQTongChieuNhan).HasPrecision(18, 4);
        builder.Property(x => x.PmaxKyHienTai).HasPrecision(18, 4);
        builder.Property(x => x.PmaxKyTruoc).HasPrecision(18, 4);

        builder.HasOne(x => x.MeasurementPoint)
            .WithMany()
            .HasForeignKey(x => x.MeasurementPointId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(Constants.TableName.MeasurementPointCurrentSnapshots);
    }
}

