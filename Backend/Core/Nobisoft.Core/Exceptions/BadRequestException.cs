using Nobisoft.Core.ValueObjects;

namespace Nobisoft.Core.Exceptions
{
    [Serializable]
    public class BadRequestException : SystemException
    {
        public BadRequestException(Tracker? tracker = null)
            : base(nameof(BadRequestException), "Your browser sent a request that this server could not understand.", tracker)
        {
        }

        public BadRequestException(string message, Tracker? tracker = null)
            : base(nameof(BadRequestException), message, tracker)
        {
        }

        public BadRequestException(Exception exception, Tracker? tracker = null)
            : base(nameof(BadRequestException), "Your browser sent a request that this server could not understand.", exception, tracker)
        {
        }

        public BadRequestException(string message, Exception exception, Tracker? tracker = null)
            : base(nameof(BadRequestException), message, exception, tracker)
        {
        }
    }
}