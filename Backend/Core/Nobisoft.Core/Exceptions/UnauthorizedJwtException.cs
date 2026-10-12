using Nobisoft.Core.ValueObjects;

namespace Nobisoft.Core.Exceptions;

public class UnauthorizedJwtException: ExceptionBase
{
    public UnauthorizedJwtException(string context, string key, string message, Tracker? tracker = null) : base(context, key, message, tracker)
    {
    }

    public UnauthorizedJwtException(string context, string key, string message, Exception exception, Tracker? tracker = null) : base(context, key, message, exception, tracker)
    {
    }
}