using UnityEngine;
using OneStore.Common.Internal;

namespace OneStore.Common
{
    public class OneStoreLogger
    {
        private const string TAG = "ONE Store: ";

        private readonly ILogger _logger = Debug.unityLogger;

        /// <summary>
        /// Logs a formatted message with ILogger.
        /// </summary>
        public void Log(string format, params object[] args)
        {
            _logger.LogFormat(LogType.Log, TAG + format, args);
        }

        /// <summary>
        /// Logs a formatted warning message with ILogger.
        /// </summary>
        public void Warning(string format, params object[] args)
        {
            _logger.LogFormat(LogType.Warning, TAG + format, args);
        }

        /// <summary>
        /// Logs a formatted error message with ILogger.
        /// </summary>
        public void Error(string format, params object[] args)
        {
            _logger.LogFormat(LogType.Error, TAG + format, args);
        }

        /// <summary>
        /// Sets the log level of the library For the convenience of development.<br/>
        /// Caution! When deploying an app, you must set the logging level to its default value.<br/>
        /// https://developer.android.com/reference/android/util/Log#summary
        /// </summary>
        /// <param name="level">default: 4 (android.util.Log.INFO)</param>
        public static void SetLogLevel(int level)
        {
            var sdkLogger = new AndroidJavaObject(Constants.SdkLogger);
            sdkLogger.CallStatic(Constants.SdkLoggerSetLogLevelMethod, level);
        }
    }
}
