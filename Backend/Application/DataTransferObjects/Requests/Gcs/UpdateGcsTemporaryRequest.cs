namespace Application.DataTransferObjects.Requests.Gcs;

public class UpdateListGcsTemporaryRequest
{
    public List<UpdateGcsTemporaryRequest> GcsTemporaryRequests { get; set; }
    public long GcsId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public int PeriodNumber { get; set; }
}


public class UpdateGcsTemporaryRequest
{
    public long Id { get; set; }
    public decimal? NewCurrentReading { get; set; }
    public decimal? NewPreviousReading { get; set; }
    public bool? IsChange { get; set; }
}

