using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace MyLogger
{
    public class Logger
    {
        private string DatetimeFormat;
        private string fileName;
        public static Logger logger;
        private static object syncRoot = new Object();

        public Logger(bool append = false)
        {
            DatetimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
            //fileName = Assembly.GetExecutingAssembly().GetName().Name + ".log";
            fileName = "log_file.log";
            string logHeader = fileName + " is created.";
            if (!File.Exists(fileName))
            {
                WriteLine(DateTime.Now.ToString(DatetimeFormat) + " " + logHeader, false);
            }
            else
            {
                if (append == false)
                    WriteLine(DateTime.Now.ToString(DatetimeFormat) + " " + logHeader, false);
            }
        }

        public static Logger getInstance(bool append = false)
        {
            if (logger == null)
            {
                lock (syncRoot)
                {
                    if (logger == null)
                        logger = new Logger(append);
                }
            }
            return logger;
        }

        public void Error(string text)
        {
            WriteFormattedLog(LogLevel.ERROR, text);
        }

        public void Info(string text)
        {
            lock (syncRoot)
            {
                WriteFormattedLog(LogLevel.INFO, text);
            }
        }

        public void Warning(string text)
        {
            WriteFormattedLog(LogLevel.WARNING, text);
        }

        private void WriteFormattedLog(LogLevel level, string text)
        {
            string pretext;
            switch (level)
            {
                case LogLevel.INFO: pretext = DateTime.Now.ToString(DatetimeFormat) + " [INFO]    "; break;
                case LogLevel.WARNING: pretext = DateTime.Now.ToString(DatetimeFormat) + " [WARNING] "; break;
                case LogLevel.ERROR: pretext = DateTime.Now.ToString(DatetimeFormat) + " [ERROR]   "; break;
                default: pretext = ""; break;
            }

            WriteLine(pretext + text);
        }

        private void WriteLine(string text, bool append = true)
        {
            try
            {
                using (StreamWriter Writer = new StreamWriter(fileName, append, Encoding.UTF8))
                {
                    if (text != "") Writer.WriteLine(text);
                }
            }
            catch
            {
                throw new IOException();
            }
        }

        [Flags]
        private enum LogLevel
        {
            INFO,
            WARNING,
            ERROR
        }
    }
}