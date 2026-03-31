using System;
using MediaBrowser.Model.Logging;
using MelLogLevel = Microsoft.Extensions.Logging.LogLevel;
using MelEventId = Microsoft.Extensions.Logging.EventId;
using MelILogger = Microsoft.Extensions.Logging.ILogger;

namespace Emby.Plugin.Sse.Logging
{
    internal class EmbyLoggerAdapter<T> : MelILogger, Microsoft.Extensions.Logging.ILogger<T>
    {
        private readonly ILogger _embyLogger;

        public EmbyLoggerAdapter(ILogger embyLogger)
        {
            _embyLogger = embyLogger;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(MelLogLevel logLevel) => true;

        public void Log<TState>(MelLogLevel logLevel, MelEventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);
            switch (logLevel)
            {
                case MelLogLevel.Critical:
                case MelLogLevel.Error:
                    if (exception != null)
                        _embyLogger.ErrorException(message, exception);
                    else
                        _embyLogger.Error(message);
                    break;
                case MelLogLevel.Warning:
                    _embyLogger.Warn(message);
                    break;
                case MelLogLevel.Information:
                    _embyLogger.Info(message);
                    break;
                default:
                    _embyLogger.Debug(message);
                    break;
            }
        }

        private class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new NullScope();
            public void Dispose() { }
        }
    }
}
