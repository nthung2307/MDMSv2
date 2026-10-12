using Nobisoft.Core.ValueObjects;

namespace Nobisoft.Core.Exceptions
{
    [Serializable]
    public class BusinessException : ExceptionBase
    {
        public int Code { get; private set; }

        public BusinessException(string context, string key, int code, string message, Tracker? tracker = null) : base(context, key, message, tracker)
        {
            Code = code;
        }

        public BusinessException(string context, string key, int code, string message, Exception exception, Tracker? tracker = null) : base(context, key, message, exception, tracker)
        {
            Code = code;
        }
        
    }
}