namespace Application.DataTransferObjects.Responses.MeterReplacement;

public class ExtractedMeterDataDto
{
    public string? OldMeterSerial { get; set; }
    public decimal OldMeterMultiplier { get; set; } = 1;
    public decimal OldBt { get; set; }
    public decimal OldCd { get; set; }
    public decimal OldTd { get; set; }
    public decimal OldSg { get; set; }
    public decimal OldVc { get; set; }
    public decimal OldPmax { get; set; }

    public string? NewMeterSerial { get; set; }
    public decimal NewMeterMultiplier { get; set; } = 1;
    public decimal NewBt { get; set; }
    public decimal NewCd { get; set; }
    public decimal NewTd { get; set; }
    public decimal NewSg { get; set; }
    public decimal NewVc { get; set; }
    public decimal NewPmax { get; set; }

    public string? Reason { get; set; }
    public string? RawAiResponse { get; set; }
}

