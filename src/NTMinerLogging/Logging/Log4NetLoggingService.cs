﻿using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Xml;

namespace NTMiner.Logging {
    public sealed class Log4NetLoggingService : ILoggingService {
        private readonly ILog _log;

        public Log4NetLoggingService() {
            XmlDocument xmlDoc = new XmlDocument();
            string logFileName = $"root{AssemblyInfo.Version}.{AssemblyInfo.Build}.log";
            string logFile = $"logs\\{logFileName}";
            if (!string.IsNullOrEmpty(LogDir.Dir)) {
                logFile = Path.Combine(LogDir.Dir, logFileName);
            }
            xmlDoc.LoadXml(
$@"<log4net>
  <root>
    <level value=""ALL"" />
    <appender-ref ref=""RollingFileAppender"" />
  </root>
  <appender name=""RollingFileAppender"" type =""log4net.Appender.RollingFileAppender"" >
    <filter type=""log4net.Filter.LevelRangeFilter"" >
    </filter>
    <param name=""File"" value =""{logFile}"" />
    <param name=""AppendToFile"" value =""true"" />
    <param name=""MaxSizeRollBackups"" value =""10"" />
    <param name=""MaximumFileSize"" value =""2MB"" />
    <param name=""RollingStyle"" value =""Size"" />
    <param name=""StaticLogFileName"" value =""true"" />
    <layout type=""log4net.Layout.PatternLayout"" >
      <param name=""ConversionPattern"" value =""%d [%t] %-5p %c - %m%n"" />
    </layout>
  </appender>
</log4net>
");
            XmlConfigurator.Configure(xmlDoc.DocumentElement);
            _log = LogManager.GetLogger("global");
        }

        public void Debug(object message) {
            Write.DevLine(message?.ToString());
            _log.Debug(message);
        }

        public void InfoDebugLine(object message) {
            Write.DevDebug(message?.ToString());
            _log.Info(message);
        }

        public void OkDebugLine(object message) {
            Write.DevOk(message?.ToString());
            _log.Info(message);
        }

        public void WarnDebugLine(object message) {
            Write.DevWarn(message?.ToString());
            _log.Warn(message);
        }

        public void ErrorDebugLine(object message) {
            Write.DevError(message?.ToString());
            _log.Error(message);
        }

        public void ErrorDebugLine(object message, Exception exception) {
            Write.DevError(message?.ToString() + exception.StackTrace);
            _log.Error(message, exception);
        }

        public void OkWriteLine(object message) {
            Write.UserOk(message?.ToString());
            _log.Info(message);
        }

        public void WarnWriteLine(object message) {
            Write.UserWarn(message?.ToString());
            _log.Warn(message);
        }

        public void EventWriteLine(object message) {
            Write.UserEvent(message?.ToString());
            _log.Info(message);
        }

        public void ErrorWriteLine(object message) {
            Write.UserError(message?.ToString());
            _log.Warn(message);
        }
    }
}
