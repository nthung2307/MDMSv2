using System;

namespace Domain.Enums;

/// <summary>
/// Bitmask flags indicating suspected / interpolated values on each of the 10 cumulative energy channels.
/// </summary>
[Flags]
public enum EnergySuspectedFlags
{
    None = 0,
    ChiSoTichLuyDienNangTacDungAPTongKenhGiao = 1 << 0,
    ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao = 1 << 1,
    ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao = 1 << 2,
    ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao = 1 << 3,
    ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao = 1 << 4,
    ChiSoTichLuyDienNangTacDungAPTongKenhNhan = 1 << 5,
    ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan = 1 << 6,
    ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan = 1 << 7,
    ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan = 1 << 8,
    ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan = 1 << 9
}

