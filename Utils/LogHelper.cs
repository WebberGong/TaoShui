using System;
using log4net;

namespace Utils
{
    public class LogHelper
    {
        public static void WriteToConsole(string msg)
        {
            Console.WriteLine(msg);
        }

        public static void LogInfo(Type type, string info)
        {
            LogManager.GetLogger(type).Info(info);
            WriteToConsole(info);
        }

        public static void LogWarn(Type type, string warn)
        {
            LogManager.GetLogger(type).Warn(warn);
            WriteToConsole(warn);
        }

        public static void LogDebug(Type type, string debug)
        {
            LogManager.GetLogger(type).Debug(debug);
            WriteToConsole(debug);
        }

        public static void LogError(Type type, string error, Exception ex = null)
        {
            LogManager.GetLogger(type).Error(error, ex);
            WriteToConsole(error + (ex == null ? "" : "\r\n" + ex.Message));
        }

        public static void LogFatal(Type type, string fatal, Exception ex = null)
        {
            LogManager.GetLogger(type).Fatal(fatal, ex);
            WriteToConsole(fatal + (ex == null ? "" : "\r\n" + ex.Message));
        }
    }
}