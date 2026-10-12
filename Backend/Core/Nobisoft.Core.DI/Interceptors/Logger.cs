using System.Diagnostics;
using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;

namespace Nobisoft.Core.DI.Interceptors
{
    public class Logger : IInterceptor
    {
        private readonly ILogger<Logger> _logger;
        private readonly TextWriter _output;

        public Logger(ILogger<Logger> logger)
        {
            ILogger<Logger> logger1 = logger;
            if (logger1 == null)
                throw new ArgumentNullException(nameof (logger));
            this._logger = logger1;
        }

        public void Intercept(IInvocation invocation)
        {
            _output.WriteLine("hello");
            Stopwatch stopwatch = Stopwatch.StartNew();
            this._logger.LogDebug("Start " + invocation.Method.Name);
            invocation.Proceed();
            Type type = invocation.InvocationTarget.GetType();
            this._logger.LogDebug("End " + invocation.Method.Name);
            this._logger.LogDebug(string.Format("{0} executed in {1} ms.", (object) type.Name, (object) stopwatch.ElapsedMilliseconds));
        }
    }
}