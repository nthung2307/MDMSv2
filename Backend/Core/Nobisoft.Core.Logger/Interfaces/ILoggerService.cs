using System.Runtime.CompilerServices;
using Nobisoft.Core.Lifetimes;

namespace Nobisoft.Core.Logger.Interfaces
{
    public interface ILoggerService : IScopedDependency
    {
        #region LogCritical

      
        public void LogFatal(Exception exception, string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args);
        public void LogFatal(string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerPath = "", [CallerLineNumber] int line = 0, params object[] args);

        #endregion

        #region LogDebug
        public void LogDebug(Exception exception, string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args);
        public void LogDebug(string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args);

        #endregion

        #region LogError
        
        public void LogError(Exception exception, string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args);
        public void LogError(string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args);

        #endregion

        #region LogInformation
        
        public void LogInformation(Exception exception, string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args);
        public void LogInformation(string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args);

        #endregion
        

        #region LogWarning
        
        public void LogWarning(Exception exception, string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args);
        public void LogWarning(string message, [CallerMemberName] string caller = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0, params object[] args);

        #endregion
    }
}