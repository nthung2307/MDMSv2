using Nobisoft.Core.Exceptions;
using Nobisoft.Core.ValueObjects;

namespace Domain.Exceptions
{
    public class UnallowedFileException : ExceptionBase
    {
        public UnallowedFileException(string context, string key, string message, Tracker? tracker = null) : base(context, key, message, tracker)
        {
        }

        public UnallowedFileException(string context, string key, string message, Exception exception, Tracker? tracker = null) : base(context, key, message, exception, tracker)
        {
        }
    }
}