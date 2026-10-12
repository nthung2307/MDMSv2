using System;
using Nobisoft.Core.ValueObjects;

namespace Nobisoft.Core.Exceptions
{
    [Serializable]
    public class SystemException : ExceptionBase
    {
        public SystemException(string context, string key, Tracker? tracker = null) : base(context, key, "Sorry, something went wrong", tracker)
        {
        }
        public SystemException(string context, string key, string message, Tracker? tracker = null) : base(context, key, message, tracker)
        {
        }

        public SystemException(string context, string key, Exception exception, Tracker? tracker = null) : base(context, key, "Sorry, something went wrong", exception, tracker)
        {
        }
        public SystemException(string context, string key, string message, Exception exception, Tracker? tracker = null) : base(context, key, message, exception, tracker)
        {
        }
    }
}