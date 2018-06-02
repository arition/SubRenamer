using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SubRenamer
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App
    {
        public App()
        {
            var config = new NLog.Config.LoggingConfiguration();
            
            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = "log.txt",
                MaxArchiveFiles = 2,
                ArchiveAboveSize = 100*1024
            };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);

            LogManager.Configuration = config;
        }
    }
}
