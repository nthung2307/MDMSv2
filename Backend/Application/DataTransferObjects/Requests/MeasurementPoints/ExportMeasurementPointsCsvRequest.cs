using Application.Common.ValueObjects.Sorting;

namespace Application.DataTransferObjects.Requests.MeasurementPoints;

public class ExportMeasurementPointsCsvRequest
{
    public string? Code { get; set; }
    public string? Status { get; set; }
    public ICollection<SearchModel>? SearchByFields { get; set; }
}
