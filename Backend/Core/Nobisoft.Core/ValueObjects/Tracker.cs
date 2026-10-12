namespace Nobisoft.Core.ValueObjects;

public class Tracker
{
    public string RequestId { get; set; }

    public Tracker()
    {
        
    }

    public Tracker(string requestId)
    {
        RequestId = requestId;
    }
}