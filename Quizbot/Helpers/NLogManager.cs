using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quizbot.Helpers
{
    class NLogManager
    {
        public static void ConfigureNLog()
        {
            LoggingConfiguration logConfig = new LoggingConfiguration();

            FileTarget logfile = new FileTarget("logfile")
            {
                FileName = "app.log",
                Layout = @"${date:format=HH\:mm\:ss} ${logger:long=True} ${level}: ${message} ${exception}",
                Encoding = Encoding.UTF8
            };

            ColoredConsoleTarget logconsole = new ColoredConsoleTarget("logconsole")
            {
                Layout = @"${date:format=HH\:mm\:ss} ${logger:long=True} ${level}: ${message} ${exception}",
            };

            logconsole.UseDefaultRowHighlightingRules = false;
            logconsole.RowHighlightingRules.Clear();
            logconsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                "level == LogLevel.Trace and starts-with('${message}','[THREAD:')", ConsoleOutputColor.Cyan, ConsoleOutputColor.Black));
            logconsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                "level == LogLevel.Trace", ConsoleOutputColor.DarkCyan, ConsoleOutputColor.Black));
            logconsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                "level == LogLevel.Debug", ConsoleOutputColor.Green, ConsoleOutputColor.Black));

            logconsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                "level == LogLevel.Info", ConsoleOutputColor.Cyan, ConsoleOutputColor.Black));

            logconsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                "level == LogLevel.Warn", ConsoleOutputColor.Yellow, ConsoleOutputColor.Black));
            logconsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(
                "level >= LogLevel.Error", ConsoleOutputColor.Red, ConsoleOutputColor.Black));

            logConfig.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logconsole);
            logConfig.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, logfile);

            LogManager.Configuration = logConfig;
        }
    }
}
