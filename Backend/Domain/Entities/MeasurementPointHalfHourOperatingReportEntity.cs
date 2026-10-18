using Nobisoft.Core.Domain.Entities;

namespace Domain.Entities;

public class MeasurementPointHalfHourOperatingReportEntity : IEntity<long>
{
    public long Id { get; set; }
    public long MeasurementPointId { get; set; }
    public MeasurementPointsEntity? MeasurementPoint { get; set; }
    public DateTimeOffset? CreatedTime { get; set; }

    public double? DienApPhaA { get; set; }
    public string? DienApPhaAUnit { get; set; }
    public double? DienApPhaB { get; set; }
    public string? DienApPhaBUnit { get; set; }
    public double? DienApPhaC { get; set; }
    public string? DienApPhaCUnit { get; set; }

    public double? DongDienPhaA { get; set; }
    public string? DongDienPhaAUnit { get; set; }
    public double? DongDienPhaB { get; set; }
    public string? DongDienPhaBUnit { get; set; }
    public double? DongDienPhaC { get; set; }
    public string? DongDienPhaCUnit { get; set; }

    public double? CongSuatTacDungTong3Pha { get; set; }
    public string? CongSuatTacDungTong3PhaUnit { get; set; }

    public double? CongSuatPhanKhangTong3Pha { get; set; }
    public string? CongSuatPhanKhangTong3PhaUnit { get; set; }

    public double? HeSoCongSuatPF { get; set; }

    public double? TanSoHeThong { get; set; }
    public string? TanSoHeThongUnit { get; set; }
}
