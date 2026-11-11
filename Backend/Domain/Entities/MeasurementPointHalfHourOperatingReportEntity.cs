using Nobisoft.Core.Domain.Entities;

namespace Domain.Entities;

public class MeasurementPointHalfHourOperatingReportEntity : IEntity<long>
{
    public long Id { get; set; }
    public long MeasurementPointId { get; set; }
    public MeasurementPointsEntity? MeasurementPoint { get; set; }
    public DateTimeOffset? CreatedTime { get; set; }

    public double? DienApPhaA { get; set; }
    public double? DienApPhaB { get; set; }
    public double? DienApPhaC { get; set; }

    public double? DongDienPhaA { get; set; }
    public double? DongDienPhaB { get; set; }
    public double? DongDienPhaC { get; set; }

    public double? CongSuatTacDungTong3Pha { get; set; }
    public double? CongSuatPhanKhangTong3Pha { get; set; }
    public double? HeSoCongSuatPF { get; set; }
    public double? TanSoHeThong { get; set; }
}
