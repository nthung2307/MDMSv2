namespace Nobisoft.Core.Exceptions;

public class InvalidStreamException: ExceptionBase
{
    public InvalidStreamException(string context, string key, string message) : base(context, key, message)
    {
    }

    public InvalidStreamException(string context, string key, string message, Exception exception) : base(context, key, message, exception)
    {
    }
}