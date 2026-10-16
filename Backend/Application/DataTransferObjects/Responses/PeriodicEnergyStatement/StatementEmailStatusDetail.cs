namespace Application.DataTransferObjects.Responses.PeriodicEnergyStatement;

public class StatementEmailStatusDetail
{
    public string Overall { get; set; } = "PENDING";
    public DateTimeOffset? SentTime { get; set; }
    public List<StatementEmailRecipientResult> Details { get; set; } = new();
}

public class StatementEmailRecipientResult
{
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = "PENDING"; // SUCCESS | FAILED | PENDING
    public string? Error { get; set; }
}
