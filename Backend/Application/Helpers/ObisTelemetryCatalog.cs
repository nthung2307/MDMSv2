using System.Globalization;
using System.Text.RegularExpressions;
using Nobisoft.Core.Constants;

namespace Application.Helpers;

public enum ObisCompositeDateTimeTarget
{
    None = 0,
    MeterTime = 1,       // 0_9_1 -> MeterHour, MeterMinute, MeterSecond
    MeterDate = 2,       // 0_9_2 -> MeterYear, MeterMonth, MeterDay
    ClosingDate = 3,     // 0_9_3 -> ClosingYear, ClosingMonth, ClosingDay
    ClosingTime = 4,     // 0_9_4 -> ClosingHour, ClosingMinute, ClosingSecond
    PmaxCurrentDate = 5, // 0_9_5 -> PmaxCurrentYear, PmaxCurrentMonth, PmaxCurrentDay
    PmaxCurrentTime = 6, // 0_9_6 -> PmaxCurrentHour, PmaxCurrentMinute, PmaxCurrentSecond
    PmaxPrevDate = 7,    // 0_9_7 -> PmaxPrevYear, PmaxPrevMonth, PmaxPrevDay
    PmaxPrevTime = 8     // 0_9_8 -> PmaxPrevHour, PmaxPrevMinute, PmaxPrevSecond
}

public sealed record ObisCatalogEntry(
    int DisplayOrder,
    string ObisCode,
    string DisplayObis,
    string ParameterCode,
    string DisplayName,
    string DataCategory,
    string? MvUnit,
    string? LvUnit,
    string[] LegacyAliases,
    string? DefaultCustomName = null,
    ObisCompositeDateTimeTarget CompositeDateTime = ObisCompositeDateTimeTarget.None);

/// <summary>
/// Canonical 60-tag OBIS catalog (IEC 62056-61), alias normalizer, and composite Date/Time parser
/// shared across Auto-Mapping, Legacy Kepware routing, and MeterProfile seeding.
/// </summary>
public static class ObisTelemetryCatalog
{
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

    public static readonly IReadOnlyList<ObisCatalogEntry> AllEntries = new List<ObisCatalogEntry>
    {
        // Group A: Operational & Electrical (19)
        new(1,  "31_7_0", "1-0:31.7.0", ListDataMeasurementPoint.DongDienPhaA, "Dòng điện pha A", "ELECTRICAL", "A", "A", new[] { "Meas.Ia", "Ia" }),
        new(2,  "51_7_0", "1-0:51.7.0", ListDataMeasurementPoint.DongDienPhaB, "Dòng điện pha B", "ELECTRICAL", "A", "A", new[] { "Meas.Ib", "Ib" }),
        new(3,  "71_7_0", "1-0:71.7.0", ListDataMeasurementPoint.DongDienPhaC, "Dòng điện pha C", "ELECTRICAL", "A", "A", new[] { "Meas.Ic", "Ic" }),
        new(4,  "32_7_0", "1-0:32.7.0", ListDataMeasurementPoint.DienApPhaA, "Điện áp pha A", "ELECTRICAL", "kV", "V", new[] { "Meas.Ua", "Meas.Va", "Ua", "Va" }),
        new(5,  "52_7_0", "1-0:52.7.0", ListDataMeasurementPoint.DienApPhaB, "Điện áp pha B", "ELECTRICAL", "kV", "V", new[] { "Meas.Ub", "Meas.Vb", "Ub", "Vb" }),
        new(6,  "72_7_0", "1-0:72.7.0", ListDataMeasurementPoint.DienApPhaC, "Điện áp pha C", "ELECTRICAL", "kV", "V", new[] { "Meas.Uc", "Meas.Vc", "Uc", "Vc" }),
        new(7,  "21_7_0", "1-0:21.7.0", ListDataMeasurementPoint.CongSuatTacDungPhaA, "Công suất tác dụng pha A", "ELECTRICAL", "kW", "kW", new[] { "Meas.Pa", "Pa" }),
        new(8,  "41_7_0", "1-0:41.7.0", ListDataMeasurementPoint.CongSuatTacDungPhaB, "Công suất tác dụng pha B", "ELECTRICAL", "kW", "kW", new[] { "Meas.Pb", "Pb" }),
        new(9,  "61_7_0", "1-0:61.7.0", ListDataMeasurementPoint.CongSuatTacDungPhaC, "Công suất tác dụng pha C", "ELECTRICAL", "kW", "kW", new[] { "Meas.Pc", "Pc" }),
        new(10, "1_7_0",  "1-0:1.7.0",  ListDataMeasurementPoint.CongSuatTacDungTong3Pha, "Công suất tác dụng tổng 3 pha", "ELECTRICAL", "kW", "kW", new[] { "Meas.Pt", "Meas.Ptotal", "15_7_0", "16_7_0" }),
        new(11, "23_7_0", "1-0:23.7.0", ListDataMeasurementPoint.CongSuatPhanKhangPhaA, "Công suất phản kháng pha A", "ELECTRICAL", "kVAr", "kVAr", new[] { "Meas.Qa", "Qa" }),
        new(12, "43_7_0", "1-0:43.7.0", ListDataMeasurementPoint.CongSuatPhanKhangPhaB, "Công suất phản kháng pha B", "ELECTRICAL", "kVAr", "kVAr", new[] { "Meas.Qb", "Qb" }),
        new(13, "63_7_0", "1-0:63.7.0", ListDataMeasurementPoint.CongSuatPhanKhangPhaC, "Công suất phản kháng pha C", "ELECTRICAL", "kVAr", "kVAr", new[] { "Meas.Qc", "Qc" }),
        new(14, "3_7_0",  "1-0:3.7.0",  ListDataMeasurementPoint.CongSuatPhanKhangTong3Pha, "Công suất phản kháng tổng 3 pha", "ELECTRICAL", "kVAr", "kVAr", new[] { "Meas.Qt", "Meas.Qtotal" }),
        new(15, "33_7_0", "1-0:33.7.0", ListDataMeasurementPoint.HeSoCongSuatPFPhaA, "Hệ số công suất PF pha A", "ELECTRICAL", null, null, new[] { "Meas.PFa", "Meas.CosPhiA" }),
        new(16, "53_7_0", "1-0:53.7.0", ListDataMeasurementPoint.HeSoCongSuatPFPhaB, "Hệ số công suất PF pha B", "ELECTRICAL", null, null, new[] { "Meas.PFb", "Meas.CosPhiB" }),
        new(17, "73_7_0", "1-0:73.7.0", ListDataMeasurementPoint.HeSoCongSuatPFPhaC, "Hệ số công suất PF pha C", "ELECTRICAL", null, null, new[] { "Meas.PFc", "Meas.CosPhiC" }),
        new(18, "13_7_0", "1-0:13.7.0", ListDataMeasurementPoint.HeSoCongSuatPFTrungBinh3Pha, "Hệ số công suất PF trung bình 3 pha", "ELECTRICAL", null, null, new[] { "Meas.PF", "Meas.PFt", "Meas.PFavg" }),
        new(19, "14_7_0", "1-0:14.7.0", ListDataMeasurementPoint.TanSoHeThong, "Tần số hệ thống", "ELECTRICAL", "Hz", "Hz", new[] { "Meas.Hz", "Meas.Freq" }),

        // Group B: Cumulative Energy (+A/+Q = Delivery, -A/-Q = Receive) (10)
        new(20, "1_8_0", "1-0:1.8.0", ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPTong, "Chỉ số tích lũy AP Tổng (Giao)", "ENERGY", "kWh", "kWh", new[] { "Energy.PTRU", "TOU.PTOT", "Energy.PTOT" }),
        new(21, "1_8_1", "1-0:1.8.1", ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPBieuGioBT, "Chỉ số tích lũy AP Biểu 1 - BT (Giao)", "ENERGY", "kWh", "kWh", new[] { "Energy.PTR1", "TOU.PTR1" }),
        new(22, "1_8_2", "1-0:1.8.2", ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPBieuGioCD, "Chỉ số tích lũy AP Biểu 2 - CĐ (Giao)", "ENERGY", "kWh", "kWh", new[] { "Energy.PTR2", "TOU.PTR2" }),
        new(23, "1_8_3", "1-0:1.8.3", ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPBieuGioTD, "Chỉ số tích lũy AP Biểu 3 - TĐ (Giao)", "ENERGY", "kWh", "kWh", new[] { "Energy.PTR3", "TOU.PTR3" }),
        new(24, "3_8_0", "1-0:3.8.0", ListDataMeasurementPoint.ChiSoTichLuyDienNangPhanKhangAQTong, "Chỉ số tích lũy AQ Tổng (Giao)", "ENERGY", "kVArh", "kVArh", new[] { "Energy.QTRU", "TOU.QTOT", "5_8_0" }),
        new(25, "2_8_0", "1-0:2.8.0", ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPTongChieuNhan, "Chỉ số tích lũy AP Tổng (Nhận)", "ENERGY", "kWh", "kWh", new[] { "Energy.NTRU", "TOU.PTOT_N", "Energy.PImTOT" }),
        new(26, "2_8_1", "1-0:2.8.1", ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan, "Chỉ số tích lũy AP Biểu 1 - BT (Nhận)", "ENERGY", "kWh", "kWh", new[] { "Energy.NTR1", "TOU.PTR1_N" }),
        new(27, "2_8_2", "1-0:2.8.2", ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan, "Chỉ số tích lũy AP Biểu 2 - CĐ (Nhận)", "ENERGY", "kWh", "kWh", new[] { "Energy.NTR2", "TOU.PTR2_N" }),
        new(28, "2_8_3", "1-0:2.8.3", ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan, "Chỉ số tích lũy AP Biểu 3 - TĐ (Nhận)", "ENERGY", "kWh", "kWh", new[] { "Energy.NTR3", "TOU.PTR3_N" }),
        new(29, "4_8_0", "1-0:4.8.0", ListDataMeasurementPoint.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan, "Chỉ số tích lũy AQ Tổng (Nhận)", "ENERGY", "kVArh", "kVArh", new[] { "Energy.QImTOT", "TOU.QTOT_N", "8_8_0" }),

        // Group C: Billing Reset Energy (F = 1) (10)
        new(30, "1_8_0_1", "1-0:1.8.0*1", ListDataMeasurementPoint.ChiSoChotDienNangAPTong, "Chỉ số chốt AP Tổng (Giao)", "ENERGY", "kWh", "kWh", new[] { "Energy.PBRU", "Billing.PTOT", "TOU.PBTOT" }),
        new(31, "1_8_1_1", "1-0:1.8.1*1", ListDataMeasurementPoint.ChiSoChotDienNangAPGioBT, "Chỉ số chốt AP Giờ BT (Giao)", "ENERGY", "kWh", "kWh", new[] { "Energy.PBR1", "TOU.PBR1", "Billing.PBR1" }),
        new(32, "1_8_2_1", "1-0:1.8.2*1", ListDataMeasurementPoint.ChiSoChotDienNangAPGioCD, "Chỉ số chốt AP Giờ CĐ (Giao)", "ENERGY", "kWh", "kWh", new[] { "Energy.PBR2", "TOU.PBR2", "Billing.PBR2" }),
        new(33, "1_8_3_1", "1-0:1.8.3*1", ListDataMeasurementPoint.ChiSoChotDienNangAPGioTD, "Chỉ số chốt AP Giờ TĐ (Giao)", "ENERGY", "kWh", "kWh", new[] { "Energy.PBR3", "TOU.PBR3", "Billing.PBR3" }),
        new(34, "3_8_0_1", "1-0:3.8.0*1", ListDataMeasurementPoint.ChiSoChotDienNangAQTong, "Chỉ số chốt AQ Tổng (Giao)", "ENERGY", "kVArh", "kVArh", new[] { "Energy.QBRU", "Billing.QTOT", "TOU.QBTOT" }),
        new(35, "2_8_0_1", "1-0:2.8.0*1", ListDataMeasurementPoint.ChiSoChotDienNangAPTongChieuNhan, "Chỉ số chốt AP Tổng (Nhận)", "ENERGY", "kWh", "kWh", new[] { "Energy.NBRU", "Billing.PTOT_N" }),
        new(36, "2_8_1_1", "1-0:2.8.1*1", ListDataMeasurementPoint.ChiSoChotDienNangAPGioBTChieuNhan, "Chỉ số chốt AP Giờ BT (Nhận)", "ENERGY", "kWh", "kWh", new[] { "Energy.NBR1", "Billing.PBR1_N" }),
        new(37, "2_8_2_1", "1-0:2.8.2*1", ListDataMeasurementPoint.ChiSoChotDienNangAPGioCDChieuNhan, "Chỉ số chốt AP Giờ CĐ (Nhận)", "ENERGY", "kWh", "kWh", new[] { "Energy.NBR2", "Billing.PBR2_N" }),
        new(38, "2_8_3_1", "1-0:2.8.3*1", ListDataMeasurementPoint.ChiSoChotDienNangAPGioTDChieuNhan, "Chỉ số chốt AP Giờ TĐ (Nhận)", "ENERGY", "kWh", "kWh", new[] { "Energy.NBR3", "Billing.PBR3_N" }),
        new(39, "4_8_0_1", "1-0:4.8.0*1", ListDataMeasurementPoint.ChiSoChotDienNangAQTongChieuNhan, "Chỉ số chốt AQ Tổng (Nhận)", "ENERGY", "kVArh", "kVArh", new[] { "Billing.QTOT_N" }),

        // Group D: Maximum Demand Pmax (2)
        new(40, "1_6_0",   "1-0:1.6.0",   ListDataMeasurementPoint.PmaxKyHienTai, "Công suất cực đại Pmax kỳ hiện tại", "ENERGY", "kW", "kW", new[] { "MD.Current", "MaxDemand.MDCU" }),
        new(41, "1_6_0_1", "1-0:1.6.0*1", ListDataMeasurementPoint.PmaxKyTruoc, "Công suất cực đại Pmax kỳ trước", "ENERGY", "kW", "kW", new[] { "MD.Previous", "MaxDemand.MDPR" }),

        // Group E: Grouped Date & Time (IEC 62056-61 clock 0_9_1 Time / 0_9_2 Date + numeric 0_9_3..0_9_8) (8)
        new(42, "0_9_1", "0-0:0.9.1", "TIME_METER_CLOCK", "Thời gian công tơ hiện tại (Time)", "TIME", null, null, Array.Empty<string>(), null, ObisCompositeDateTimeTarget.MeterTime),
        new(43, "0_9_2", "0-0:0.9.2", "DATE_METER_CLOCK", "Ngày công tơ hiện tại (Date)", "TIME", null, null, Array.Empty<string>(), null, ObisCompositeDateTimeTarget.MeterDate),
        new(44, "0_9_3", "0-0:0.1.2 (Date)", "DATE_CLOSING_LAST", "Ngày chốt chỉ số gần nhất (Date)", "TIME", null, null, new[] { "0_1_2_D" }, null, ObisCompositeDateTimeTarget.ClosingDate),
        new(45, "0_9_4", "0-0:0.1.2 (Time)", "TIME_CLOSING_LAST", "Giờ chốt chỉ số gần nhất (Time)", "TIME", null, null, new[] { "0_1_2_T" }, null, ObisCompositeDateTimeTarget.ClosingTime),
        new(46, "0_9_5", "1-0:1.6.0 (Date)", "DATE_PMAX_CURRENT", "Ngày Pmax kỳ hiện tại (Date)", "TIME", null, null, new[] { "1_6_0_D" }, null, ObisCompositeDateTimeTarget.PmaxCurrentDate),
        new(47, "0_9_6", "1-0:1.6.0 (Time)", "TIME_PMAX_CURRENT", "Giờ Pmax kỳ hiện tại (Time)", "TIME", null, null, new[] { "1_6_0_T" }, null, ObisCompositeDateTimeTarget.PmaxCurrentTime),
        new(48, "0_9_7", "1-0:1.6.0*1 (Date)", "DATE_PMAX_PREVIOUS", "Ngày Pmax kỳ trước (Date)", "TIME", null, null, new[] { "1_6_0_1_D" }, null, ObisCompositeDateTimeTarget.PmaxPrevDate),
        new(49, "0_9_8", "1-0:1.6.0*1 (Time)", "TIME_PMAX_PREVIOUS", "Giờ Pmax kỳ trước (Time)", "TIME", null, null, new[] { "1_6_0_1_T" }, null, ObisCompositeDateTimeTarget.PmaxPrevTime),

        // Group F: Status & Alerts (11)
        new(50, "96_5_0",   "0-0:96.5.0",   ListDataMeasurementPoint.MatKetNoi,  "Mất kết nối công tơ", "ALERT", null, null, new[] { "Status.Error", "Disconnected", "_System._Error" }, "Mất kết nối công tơ"),
        new(51, "97_97_1",  "0-0:97.97.1",  ListDataMeasurementPoint.CanhBao01, "Cảnh báo 01 (EFA01)", "ALERT", null, null, new[] { "Status.Efa01", "Efa.Efa01" }, "Cảnh báo 01"),
        new(52, "97_97_2",  "0-0:97.97.2",  ListDataMeasurementPoint.CanhBao02, "Cảnh báo 02 (EFA02)", "ALERT", null, null, new[] { "Status.Efa02", "Efa.Efa02" }, "Cảnh báo 02"),
        new(53, "97_97_3",  "0-0:97.97.3",  ListDataMeasurementPoint.CanhBao03, "Cảnh báo 03 (EFA03)", "ALERT", null, null, new[] { "Status.Efa03", "Efa.Efa03" }, "Cảnh báo 03"),
        new(54, "97_97_4",  "0-0:97.97.4",  ListDataMeasurementPoint.CanhBao04, "Cảnh báo 04 (EFA04)", "ALERT", null, null, new[] { "Status.Efa04", "Efa.Efa04" }, "Cảnh báo 04"),
        new(55, "97_97_5",  "0-0:97.97.5",  ListDataMeasurementPoint.CanhBao05, "Cảnh báo 05 (EFA05)", "ALERT", null, null, new[] { "Status.Efa05", "Efa.Efa05" }, "Cảnh báo 05"),
        new(56, "97_97_6",  "0-0:97.97.6",  ListDataMeasurementPoint.CanhBao06, "Cảnh báo 06 (EFA06)", "ALERT", null, null, new[] { "Status.Efa06", "Efa.Efa06" }, "Cảnh báo 06"),
        new(57, "97_97_7",  "0-0:97.97.7",  ListDataMeasurementPoint.CanhBao07, "Cảnh báo 07 (EFA07)", "ALERT", null, null, new[] { "Status.Efa07", "Efa.Efa07" }, "Cảnh báo 07"),
        new(58, "97_97_8",  "0-0:97.97.8",  ListDataMeasurementPoint.CanhBao08, "Cảnh báo 08", "ALERT", null, null, new[] { "Status.Efa08", "Efa.Efa08" }, "Cảnh báo 08"),
        new(59, "97_97_9",  "0-0:97.97.9",  ListDataMeasurementPoint.CanhBao09, "Cảnh báo 09", "ALERT", null, null, new[] { "Status.Efa09", "Efa.Efa09" }, "Cảnh báo 09"),
        new(60, "97_97_10", "0-0:97.97.10", ListDataMeasurementPoint.CanhBao10, "Cảnh báo 10", "ALERT", null, null, new[] { "Status.Efa10", "Efa.Efa10" }, "Cảnh báo 10"),
    };

    // Legacy 24 individual DateTime Kepware suffixes (Datetime.csv backward compatibility)
    public static readonly IReadOnlyDictionary<string, string> LegacyDateTimeSuffixToParameter =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["DT.Current.Year"]   = ListDataMeasurementPoint.NamHienTai,
            ["DT.Current.Month"]  = ListDataMeasurementPoint.ThangHienTai,
            ["DT.Current.Day"]    = ListDataMeasurementPoint.NgayHienTai,
            ["DT.Current.Hour"]   = ListDataMeasurementPoint.GioHienTai,
            ["DT.Current.Minute"] = ListDataMeasurementPoint.PhutHienTai,
            ["DT.Current.Second"] = ListDataMeasurementPoint.GiayHienTai,

            ["DT.Reset.Year"]   = ListDataMeasurementPoint.NamChotGanNhat,
            ["DT.Reset.Month"]  = ListDataMeasurementPoint.ThangChotGanNhat,
            ["DT.Reset.Day"]    = ListDataMeasurementPoint.NgayChotGanNhat,
            ["DT.Reset.Hour"]   = ListDataMeasurementPoint.GioChotGanNhat,
            ["DT.Reset.Minute"] = ListDataMeasurementPoint.PhutChotGanNhat,
            ["DT.Reset.Second"] = ListDataMeasurementPoint.GiayChotGanNhat,

            ["DT.MDC.Year"]   = ListDataMeasurementPoint.NamPmaxKyHienTai,
            ["DT.MDC.Month"]  = ListDataMeasurementPoint.ThangPmaxKyHienTai,
            ["DT.MDC.Day"]    = ListDataMeasurementPoint.NgayPmaxKyHienTai,
            ["DT.MDC.Hour"]   = ListDataMeasurementPoint.GioPmaxKyHienTai,
            ["DT.MDC.Minute"] = ListDataMeasurementPoint.PhutPmaxKyHienTai,
            ["DT.MDC.Second"] = ListDataMeasurementPoint.GiayPmaxKyHienTai,

            ["DT.MDP.Year"]   = ListDataMeasurementPoint.NamPmaxKyTruoc,
            ["DT.MDP.Month"]  = ListDataMeasurementPoint.ThangPmaxKyTruoc,
            ["DT.MDP.Day"]    = ListDataMeasurementPoint.NgayPmaxKyTruoc,
            ["DT.MDP.Hour"]   = ListDataMeasurementPoint.GioPmaxKyTruoc,
            ["DT.MDP.Minute"] = ListDataMeasurementPoint.PhutPmaxKyTruoc,
            ["DT.MDP.Second"] = ListDataMeasurementPoint.GiayPmaxKyTruoc,
        };

    public static readonly IReadOnlyDictionary<string, ObisCatalogEntry> ByObisCode;
    public static readonly IReadOnlyDictionary<string, ObisCatalogEntry> ByParameterCode;
    public static readonly IReadOnlyDictionary<string, ObisCatalogEntry> ByLegacyAlias;

    static ObisTelemetryCatalog()
    {
        var obisMap = new Dictionary<string, ObisCatalogEntry>(StringComparer.OrdinalIgnoreCase);
        var paramMap = new Dictionary<string, ObisCatalogEntry>(StringComparer.OrdinalIgnoreCase);
        var aliasMap = new Dictionary<string, ObisCatalogEntry>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in AllEntries)
        {
            obisMap[entry.ObisCode] = entry;
            paramMap[entry.ParameterCode] = entry;

            foreach (var alias in entry.LegacyAliases)
            {
                var normalizedAlias = NormalizeAliasKey(alias);
                aliasMap[normalizedAlias] = entry;

                // If alias looks like an OBIS code (e.g. 15_7_0, 0_1_2_D), register in obisMap too when absent
                if (!obisMap.ContainsKey(normalizedAlias) && LooksLikeObisUnderscore(normalizedAlias))
                {
                    obisMap[normalizedAlias] = entry;
                }
            }
        }

        ByObisCode = obisMap;
        ByParameterCode = paramMap;
        ByLegacyAlias = aliasMap;
    }

    public static bool LooksLikeObisUnderscore(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var parts = value.Split('_');
        if (parts.Length < 3 || parts.Length > 4) return false;
        return parts.All(p => p.Length > 0 && p.Length <= 3 && p.All(char.IsDigit));
    }

    public static string NormalizeAliasKey(string rawAlias)
        => string.IsNullOrWhiteSpace(rawAlias)
            ? string.Empty
            : WhitespaceRegex.Replace(rawAlias.Trim(), string.Empty);

    /// <summary>
    /// Extracts and canonicalizes an OBIS code (e.g., "1-0:1.8.0*255", "M010.1_8_0", "1.8.0*1" -> "1_8_0" / "1_8_0_1").
    /// Returns null if the raw tag ID does not contain a recognizable OBIS tail.
    /// </summary>
    public static string? NormalizeObisCode(string? rawTagId)
    {
        if (string.IsNullOrWhiteSpace(rawTagId)) return null;
        var trimmed = rawTagId.Trim();

        // Format with A-B:C.D.E*F (IEC 62056 colon delimiter)
        var colonIdx = trimmed.LastIndexOf(':');
        if (colonIdx >= 0 && colonIdx + 1 < trimmed.Length)
        {
            var afterColon = trimmed[(colonIdx + 1)..];
            var tokens = afterColon.Split(new[] { '.', '*', '_' }, StringSplitOptions.RemoveEmptyEntries);
            return CanonicalizeNumericObisTokens(tokens);
        }

        // Last dot-segment may already use underscore OBIS format (e.g., _AdvancedTags.M010.1_8_0 or 97_97_10)
        var dotParts = trimmed.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (dotParts.Length > 0)
        {
            var lastSegment = dotParts[^1];
            if (lastSegment.Contains('_'))
            {
                var underscoreTokens = lastSegment.Split('_', StringSplitOptions.RemoveEmptyEntries);
                var candidate = CanonicalizeNumericObisTokens(underscoreTokens);
                if (candidate != null) return candidate;
            }
        }

        // Trailing dot-separated / star-separated numeric OBIS tokens (e.g., "Device1.1.8.0*1" or "1.8.0")
        var tailRawTokens = trimmed.Split(new[] { '.', '*' }, StringSplitOptions.RemoveEmptyEntries);
        var numericTail = new List<string>(4);
        for (int i = tailRawTokens.Length - 1; i >= 0 && numericTail.Count < 4; i--)
        {
            var seg = tailRawTokens[i];
            if (seg.Length > 0 && seg.Length <= 3 && seg.All(char.IsDigit))
            {
                numericTail.Add(seg);
            }
            else
            {
                break;
            }
        }

        if (numericTail.Count >= 3)
        {
            numericTail.Reverse();
            // Prefer 4-token match first (if billing period *1 or *255), otherwise check 3-token match
            if (numericTail.Count == 4)
            {
                var asFour = CanonicalizeNumericObisTokens(numericTail.ToArray());
                if (asFour != null && ByObisCode.ContainsKey(asFour))
                    return asFour;
                return CanonicalizeNumericObisTokens(numericTail.Skip(1).ToArray());
            }

            return CanonicalizeNumericObisTokens(numericTail.ToArray());
        }

        return null;
    }

    private static string? CanonicalizeNumericObisTokens(string[] tokens)
    {
        if (tokens.Length < 3 || tokens.Length > 4) return null;
        if (!tokens.All(t => t.Length > 0 && t.Length <= 3 && t.All(char.IsDigit))) return null;

        // Parse integers to remove unnecessary leading zeros (e.g., "01_08_00" -> "1_8_0")
        var ints = tokens.Select(t => int.Parse(t, CultureInfo.InvariantCulture)).ToArray();

        // Standard IEC current value billing specifier *255 or .255 strips to base 3-part OBIS
        if (ints.Length == 4 && ints[3] == 255)
        {
            return $"{ints[0]}_{ints[1]}_{ints[2]}";
        }

        return ints.Length == 3
            ? $"{ints[0]}_{ints[1]}_{ints[2]}"
            : $"{ints[0]}_{ints[1]}_{ints[2]}_{ints[3]}";
    }

    /// <summary>
    /// Strips optional Kepware Channel/Device prefixes (e.g., "_AdvancedTags.M010.Energy.PTRU" -> "Energy.PTRU")
    /// and attempts suffix matching against canonical legacy aliases or 24 individual DateTime tags.
    /// </summary>
    public static bool TryMatchLegacySuffix(string rawTagId, out string? obisOrParameterCode, out ObisCatalogEntry? catalogEntry, out string? legacyDateTimeParameter)
    {
        obisOrParameterCode = null;
        catalogEntry = null;
        legacyDateTimeParameter = null;

        if (string.IsNullOrWhiteSpace(rawTagId)) return false;

        var segments = rawTagId.Trim().Split('.', StringSplitOptions.RemoveEmptyEntries);
        for (int start = 0; start < segments.Length; start++)
        {
            var suffix = string.Join('.', segments, start, segments.Length - start);
            if (LegacyDateTimeSuffixToParameter.TryGetValue(suffix, out var dtParam))
            {
                legacyDateTimeParameter = dtParam;
                obisOrParameterCode = dtParam;
                return true;
            }

            if (ByLegacyAlias.TryGetValue(suffix, out var entry))
            {
                catalogEntry = entry;
                obisOrParameterCode = entry.ParameterCode;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Parses a grouped Date or Time value into component shorts (Year/Month/Day or Hour/Minute/Second)
    /// with format auto-detection for ISO yyyy-MM-dd, dd/MM/yyyy, yyyyMMdd, HH:mm:ss, HH:mm, HHmmss.
    /// </summary>
    public static bool TryDecomposeCompositeDateTime(
        ObisCompositeDateTimeTarget target,
        string? rawValue,
        out (short P1, short P2, short P3) parts)
    {
        parts = default;
        if (target == ObisCompositeDateTimeTarget.None || string.IsNullOrWhiteSpace(rawValue))
            return false;

        var val = rawValue.Trim();
        var isDateTarget = target is ObisCompositeDateTimeTarget.MeterDate
            or ObisCompositeDateTimeTarget.ClosingDate
            or ObisCompositeDateTimeTarget.PmaxCurrentDate
            or ObisCompositeDateTimeTarget.PmaxPrevDate;

        if (isDateTarget)
        {
            // Support exact formats first
            string[] dateFormats =
            {
                "yyyy-MM-dd", "yyyy/MM/dd", "dd/MM/yyyy", "dd-MM-yyyy",
                "d/M/yyyy", "d-M-yyyy", "yyyyMMdd"
            };

            if (DateTime.TryParseExact(val, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                return ValidateAndAssignDate(dt, out parts);
            }

            // Fallback: full ISO DateTime string e.g. 2026-11-12T10:30:00Z
            if (DateTime.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dt))
            {
                return ValidateAndAssignDate(dt, out parts);
            }

            return false;
        }
        else
        {
            string[] timeFormats = { "HH:mm:ss", "H:mm:ss", "HH:mm", "H:mm", "HHmmss", "HHmm", "HH:mm:ss.fff" };
            if (DateTime.TryParseExact(val, timeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var tm))
            {
                parts = ((short)tm.Hour, (short)tm.Minute, (short)tm.Second);
                return true;
            }

            if (TimeSpan.TryParse(val, CultureInfo.InvariantCulture, out var ts) && ts.Hours is >= 0 and <= 23)
            {
                parts = ((short)ts.Hours, (short)ts.Minutes, (short)ts.Seconds);
                return true;
            }

            if (DateTime.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out tm))
            {
                parts = ((short)tm.Hour, (short)tm.Minute, (short)tm.Second);
                return true;
            }

            return false;
        }
    }

    private static bool ValidateAndAssignDate(DateTime dt, out (short Year, short Month, short Day) parts)
    {
        if (dt.Year < 2000 || dt.Year > 2100)
        {
            parts = default;
            return false;
        }
        parts = ((short)dt.Year, (short)dt.Month, (short)dt.Day);
        return true;
    }

    public static IReadOnlyList<ObisCompositeDateTimeTarget> GetDateTimeTargets()
        => new[]
        {
            ObisCompositeDateTimeTarget.MeterTime,
            ObisCompositeDateTimeTarget.MeterDate,
            ObisCompositeDateTimeTarget.ClosingDate,
            ObisCompositeDateTimeTarget.ClosingTime,
            ObisCompositeDateTimeTarget.PmaxCurrentDate,
            ObisCompositeDateTimeTarget.PmaxCurrentTime,
            ObisCompositeDateTimeTarget.PmaxPrevDate,
            ObisCompositeDateTimeTarget.PmaxPrevTime,
        };

    public static (string Param1, string Param2, string Param3) GetSnapshotDateTimeComponentParameters(ObisCompositeDateTimeTarget target)
        => target switch
        {
            ObisCompositeDateTimeTarget.MeterTime => (ListDataMeasurementPoint.GioHienTai, ListDataMeasurementPoint.PhutHienTai, ListDataMeasurementPoint.GiayHienTai),
            ObisCompositeDateTimeTarget.MeterDate => (ListDataMeasurementPoint.NamHienTai, ListDataMeasurementPoint.ThangHienTai, ListDataMeasurementPoint.NgayHienTai),
            ObisCompositeDateTimeTarget.ClosingDate => (ListDataMeasurementPoint.NamChotGanNhat, ListDataMeasurementPoint.ThangChotGanNhat, ListDataMeasurementPoint.NgayChotGanNhat),
            ObisCompositeDateTimeTarget.ClosingTime => (ListDataMeasurementPoint.GioChotGanNhat, ListDataMeasurementPoint.PhutChotGanNhat, ListDataMeasurementPoint.GiayChotGanNhat),
            ObisCompositeDateTimeTarget.PmaxCurrentDate => (ListDataMeasurementPoint.NamPmaxKyHienTai, ListDataMeasurementPoint.ThangPmaxKyHienTai, ListDataMeasurementPoint.NgayPmaxKyHienTai),
            ObisCompositeDateTimeTarget.PmaxCurrentTime => (ListDataMeasurementPoint.GioPmaxKyHienTai, ListDataMeasurementPoint.PhutPmaxKyHienTai, ListDataMeasurementPoint.GiayPmaxKyHienTai),
            ObisCompositeDateTimeTarget.PmaxPrevDate => (ListDataMeasurementPoint.NamPmaxKyTruoc, ListDataMeasurementPoint.ThangPmaxKyTruoc, ListDataMeasurementPoint.NgayPmaxKyTruoc),
            ObisCompositeDateTimeTarget.PmaxPrevTime => (ListDataMeasurementPoint.GioPmaxKyTruoc, ListDataMeasurementPoint.PhutPmaxKyTruoc, ListDataMeasurementPoint.GiayPmaxKyTruoc),
            _ => (string.Empty, string.Empty, string.Empty)
        };
}

