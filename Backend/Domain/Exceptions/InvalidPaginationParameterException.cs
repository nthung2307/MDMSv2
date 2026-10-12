using Nobisoft.Core.Exceptions;
using Nobisoft.Core.ValueObjects;

namespace Domain.Exceptions;

public class InvalidPaginationParameterException: ExceptionBase
{
    public InvalidPaginationParameterException(string context, string key, string message, Tracker? tracker = null) : base(context, key, message, tracker)
    {
    }

    public InvalidPaginationParameterException(string context, string key, string message, Exception exception, Tracker? tracker = null) : base(context, key, message, exception, tracker)
    {
    }
}