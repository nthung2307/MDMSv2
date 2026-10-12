using System.Net;
using System.Reflection;

namespace Nobisoft.Core.AspNetCore.Http
{
    public class HttpResponseMessage
    {
        public HttpStatusCode Status { get; }

        public object Payload { get; }

        public string Message { get; }

        internal HttpResponseMessage(HttpStatusCode status, object payload)
        {
            Status = status;
            Payload = payload;
        }

        internal HttpResponseMessage(HttpStatusCode status, string message, object payload)
        {
            Status = status;
            Message = message;
            Payload = payload;
        }

        public static HttpResponseMessage Success(object payload)
        {
            return new HttpResponseMessage(HttpStatusCode.OK, payload);
        }

        public static HttpResponseMessage Success(string message, object payload)
        {
            return new HttpResponseMessage(HttpStatusCode.OK, message, payload);
        }

        public static HttpResponseMessage Fail(HttpStatusCode status, string message)
        {
            return new HttpResponseMessage(status, message, (object) null);
        }

        public static HttpResponseMessage Fail(HttpStatusCode status, string message, object payload)
        {
            return new HttpResponseMessage(status, message, payload);
        }

        public static HttpResponseMessage Fail(HttpStatusCode status, Exception exception)
        {
            var data = exception.Data;
            return new HttpResponseMessage(status, exception.Message, (object) data);
        }

        internal bool IsSimpleObject(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
                return IsSimpleObject(typeInfo.GetGenericArguments()[0]);
            return typeInfo.IsPrimitive || typeInfo.IsEnum || object.Equals((object) type, (object) typeof(string)) || object.Equals((object) type, (object) typeof(Decimal));
        }
    }
}