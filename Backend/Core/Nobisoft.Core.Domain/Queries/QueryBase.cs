using Nobisoft.Core.ValueObjects;

namespace Nobisoft.Core.Domain.Queries;

public class QueryBase
{
    public QueryBase(Tracker tracker)
    {
        Tracker = tracker;
    }

    public QueryBase()
    {
        
    }
    public Tracker Tracker { get; set; }
    
}