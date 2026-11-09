using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Nobisoft.Core.Logger.Interfaces;
using Serilog;
using Serilog.Context;

namespace Nobisoft.Core.Logger.Implementations
{
    public class LoggerService : ILoggerService
    {
        private readonly ILogger<LoggerService> _logger;


        public LoggerService(ILogger<LoggerService> logger)
        {
            _logger = logger;
        }
        

        public void LogFatal(Exception exception, string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args)
        {
            using (LogContext.PushProperty("CallerMemberName", caller))
            using (LogContext.PushProperty("CallerFilePath", callerFilePath))
            using (LogContext.PushProperty("CallerLineNumber", callerLineNumber))
            {
                Log.Fatal(exception, message, args);
            }
        }

        public void LogFatal(string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args)
        {
            using (LogContext.PushProperty("CallerMemberName", caller))
            using (LogContext.PushProperty("CallerFilePath", callerFilePath))
            using (LogContext.PushProperty("CallerLineNumber", callerLineNumber))
            {
                Log.Fatal(message, args);
            }
        }


        public void LogDebug(Exception exception, string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                using (LogContext.PushProperty("CallerMemberName", caller))
                using (LogContext.PushProperty("CallerFilePath", callerFilePath))
                using (LogContext.PushProperty("CallerLineNumber", callerLineNumber))
                {
                    Log.Debug(exception, message, args);
                }
            }
        }

        public void LogDebug(string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                using (LogContext.PushProperty("CallerMemberName", caller))
                using (LogContext.PushProperty("CallerFilePath", callerFilePath))
                using (LogContext.PushProperty("CallerLineNumber", callerLineNumber))
                {
                    Log.Debug(message, args);
                }
            }
        }


        public void LogError(Exception exception, string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args)
        {
            using (LogContext.PushProperty("CallerMemberName", caller))
            using (LogContext.PushProperty("CallerFilePath", callerFilePath))
            using (LogContext.PushProperty("CallerLineNumber", callerLineNumber))
            {
                Log.Error(exception, message, args);
            }
        }

        public void LogError(string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args)
        {
            using (LogContext.PushProperty("CallerMemberName", caller))
            using (LogContext.PushProperty("CallerFilePath", callerFilePath))
            using (LogContext.PushProperty("CallerLineNumber", callerLineNumber))
            {
                Log.Error(message, args);
            }
        }


        public void LogInformation(Exception exception, string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args)
        {
            using (LogContext.PushProperty("CallerMemberName", caller))
            using (LogContext.PushProperty("CallerFilePath", callerFilePath))
            using (LogContext.PushProperty("CallerLineNumber", callerLineNumber))
            {
                Log.Information(exception, message, args);
            }
        }

        public void LogInformation(string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args)
        {
            using (LogContext.PushProperty("CallerMemberName", caller))
            using (LogContext.PushProperty("CallerFilePath", callerFilePath))
            using (LogContext.PushProperty("CallerLineNumber", callerLineNumber))
            {
                Log.Information(message, args);
            }
        }


        public void LogWarning(Exception exception, string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args)
        {
            using (LogContext.PushProperty("CallerMemberName", caller))
            using (LogContext.PushProperty("CallerFilePath", callerFilePath))
            using (LogContext.PushProperty("CallerLineNumber", callerLineNumber))
            {
                Log.Warning(exception, message, args);
            }
        }

        public void LogWarning(string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args)
        {
            using (LogContext.PushProperty("CallerMemberName", caller))
            using (LogContext.PushProperty("CallerFilePath", callerFilePath))
            using (LogContext.PushProperty("CallerLineNumber", callerLineNumber))
            {
                Log.Warning(message, args);
            }
        }
    }
}