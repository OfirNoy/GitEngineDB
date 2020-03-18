using System;
using Serilog;

namespace GitEngineDB.Logging
{
    internal static class DbLogger
    {
        private static ILogger _log;

        public static void Init(ILogger logger)
        {
            _log = logger;
        }
        public static void Debug(string message, params string[] p)
        {
            _log?.Debug(message, p);
        }
        public static void Debug(string message)
        {
            _log?.Debug(message);
        }
        public static void Warn(string message, params string[] p)
        {
            _log?.Warning(message, p);
        }
        public static void Warn(string message)
        {
            _log?.Warning(message);
        }
        public static void Error(string message, params string[] p)
        {
            _log?.Error(message, p);
        }
        public static void Error(string message)
        {
            _log?.Error(message);
        }
        public static void Error(Exception ex, string message = "")
        {
            _log?.Error(ex, message);
        }
    }
}