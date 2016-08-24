using System;
using log4net;

namespace Utils
{
    public class LogHelper
    {
        private static LogHelper _instance;
        private static readonly object locker = new object();

        private LogHelper()
        {
        }

        public static LogHelper Instance
        {
            get
            {
                if (_instance == null)
                    lock (locker)
                    {
                        if (_instance == null)
                            _instance = new LogHelper();
                    }
                return _instance;
            }
        }

        public void WriteToConsole(string msg)
        {
            Console.WriteLine(msg);
        }

        public void LogInfo(Type type, string info)
        {
            LogManager.GetLogger(type).Info(info);
            WriteToConsole(info);
        }

        public void LogWarn(Type type, string warn)
        {
            LogManager.GetLogger(type).Warn(warn);
            WriteToConsole(warn);
        }

        public void LogDebug(Type type, string debug)
        {
            LogManager.GetLogger(type).Debug(debug);
            WriteToConsole(debug);
        }

        public void LogError(Type type, string error, Exception ex = null)
        {
            LogManager.GetLogger(type).Error(error, ex);
            WriteToConsole(error + (ex == null ? "" : "\r\n" + ex.Message));
        }

        public void LogFatal(Type type, string fatal, Exception ex = null)
        {
            LogManager.GetLogger(type).Fatal(fatal, ex);
            WriteToConsole(fatal + (ex == null ? "" : "\r\n" + ex.Message));
        }
    }
}