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
            using (GlobalLogContext.Lock())
            {
                GlobalLogContext.PushProperty("CallerMemberName", caller, true);
                GlobalLogContext.PushProperty("CallerFilePath", callerFilePath, true);
                GlobalLogContext.PushProperty("CallerLineNumber", callerLineNumber, true);
                Log.Fatal(exception, message, args);
            }
        }

        public void LogFatal(string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args)
        {
            using (GlobalLogContext.Lock())
            {
                GlobalLogContext.PushProperty("CallerMemberName", caller, true);
                GlobalLogContext.PushProperty("CallerFilePath", callerFilePath, true);
                GlobalLogContext.PushProperty("CallerLineNumber", callerLineNumber, true);
                Log.Fatal(message, args);
            }
        }


        public void LogDebug(Exception exception, string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                using (GlobalLogContext.Lock())
                {
                    GlobalLogContext.PushProperty("CallerMemberName", caller, true);
                    GlobalLogContext.PushProperty("CallerFilePath", callerFilePath, true);
                    GlobalLogContext.PushProperty("CallerLineNumber", callerLineNumber, true);
                    Log.Debug(exception, message, args);
                }
            }
        }

        public void LogDebug(string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                using (GlobalLogContext.Lock())
                {
                    GlobalLogContext.PushProperty("CallerMemberName", caller, true);
                    GlobalLogContext.PushProperty("CallerFilePath", callerFilePath, true);
                    GlobalLogContext.PushProperty("CallerLineNumber", callerLineNumber, true);
                    Log.Debug(message, args);
                }
            }
        }


        public void LogError(Exception exception, string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args)
        {
            using (GlobalLogContext.Lock())
            {
                GlobalLogContext.PushProperty("CallerMemberName", caller, true);
                GlobalLogContext.PushProperty("CallerFilePath", callerFilePath, true);
                GlobalLogContext.PushProperty("CallerLineNumber", callerLineNumber, true);
                Log.Error(exception, message, args);
            }
        }

        public void LogError(string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args)
        {
            using (GlobalLogContext.Lock())
            {
                GlobalLogContext.PushProperty("CallerMemberName", caller, true);
                GlobalLogContext.PushProperty("CallerFilePath", callerFilePath, true);
                GlobalLogContext.PushProperty("CallerLineNumber", callerLineNumber, true);
                Log.Error(message, args);
            }
        }


        public void LogInformation(Exception exception, string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args)
        {
            using (GlobalLogContext.Lock())
            {
                GlobalLogContext.PushProperty("CallerMemberName", caller, true);
                GlobalLogContext.PushProperty("CallerFilePath", callerFilePath, true);
                GlobalLogContext.PushProperty("CallerLineNumber", callerLineNumber, true);
                Log.Information(exception, message, args);
            }
        }

        public void LogInformation(string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args)
        {
            using (GlobalLogContext.Lock())
            {
                GlobalLogContext.PushProperty("CallerMemberName", caller, true);
                GlobalLogContext.PushProperty("CallerFilePath", callerFilePath, true);
                GlobalLogContext.PushProperty("CallerLineNumber", callerLineNumber, true);
                Log.Information(message, args);
            }
        }


        public void LogWarning(Exception exception, string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args)
        {
            using (GlobalLogContext.Lock())
            {
                GlobalLogContext.PushProperty("CallerMemberName", caller, true);
                GlobalLogContext.PushProperty("CallerFilePath", callerFilePath, true);
                GlobalLogContext.PushProperty("CallerLineNumber", callerLineNumber, true);
                Log.Warning(exception, message, args);
            }
        }

        public void LogWarning(string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args)
        {
            using (GlobalLogContext.Lock())
            {
                GlobalLogContext.PushProperty("CallerMemberName", caller, true);
                GlobalLogContext.PushProperty("CallerFilePath", callerFilePath, true);
                GlobalLogContext.PushProperty("CallerLineNumber", callerLineNumber, true);
                Log.Warning(message, args);
            }
        }
    }
}