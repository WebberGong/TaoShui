using System;
using log4net;

namespace TaoShui
{
    public class LogHelper
    {
        public static void LogInfo(Type type, string info)
        {
            LogManager.GetLogger(type).Info(info);
        }

        public static void LogWarn(Type type, string warn)
        {
            LogManager.GetLogger(type).Warn(warn);
        }

        public static void LogDebug(Type type, string debug)
        {
            LogManager.GetLogger(type).Debug(debug);
        }

        public static void LogError(Type type, string error, Exception ex = null)
        {
            LogManager.GetLogger(type).Error(error, ex);
        }

        public static void LogFatal(Type type, string fatal, Exception ex = null)
        {
            LogManager.GetLogger(type).Fatal(fatal, ex);
        }
    }
}