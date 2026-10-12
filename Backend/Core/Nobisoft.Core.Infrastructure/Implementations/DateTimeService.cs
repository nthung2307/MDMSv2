using Nobisoft.Core.Infrastructure.Interfaces;

namespace Nobisoft.Core.Infrastructure.Implementations;

public class DateTimeService: IDateTimeService
{
    public DateTimeOffset Now()
    {
        return DateTimeOffset.Now;
    }

    public DateTimeOffset UtcNow()
    {
        return DateTimeOffset.UtcNow;
    }
}