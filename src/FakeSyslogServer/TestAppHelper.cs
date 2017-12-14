using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NLog.Common;

namespace FakeSyslogServer
{
    public class TestAppHelper
    {
        private static readonly Logger Logger;
        private static readonly string HugeMessage;

        private readonly Func<string, string> settings;
        private readonly Action<bool, SyslogServer> toggleSyslogServer;
        private readonly SyslogServer syslogServer;

        static TestAppHelper()
        {
            var sb = new StringBuilder(65000);
            for (var i = 10; i <= 64000; i += 10)
                sb.Append(i.ToString("D10"));
            HugeMessage = sb.ToString();

            //InternalLogger.LogLevel = LogLevel.Warn;
            //InternalLogger.LogToTrace = true;

            Logger = LogManager.GetCurrentClassLogger();
        }

        public TestAppHelper(Func<string, string> settingProvider, Action<bool, SyslogServer> toggleSyslogServerFunc)
        {
            TaskScheduler.UnobservedTaskException += HandleUnobservedTaskException;
            settings = settingProvider;
            toggleSyslogServer = toggleSyslogServerFunc;
            var tcpEndPoint = EndPoint("tcpIp", "tcpPort");
            var udpEndPoint = EndPoint("udpIp", "udpPort");
            syslogServer = new SyslogServer(udpEndPoint, tcpEndPoint);
        }

        public void Dispose()
        {
            syslogServer.Dispose();
        }

        private static void HandleUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            foreach (var innerException in e.Exception.Flatten().InnerExceptions)
            {
                Trace.WriteLine("******************** DANGEROUSLY UNOBSERVED TASK EXCEPTION!!!");
                Trace.WriteLine(innerException);
            }
        }

        private IPEndPoint EndPoint(string ipKey, string portKey)
        {
            var ip = IPAddress.Parse(settings(ipKey));
            var port = int.Parse(settings(portKey));
            return new IPEndPoint(ip, port);
        }

        public void PerformSelectedOperation(string operation)
        {
            switch (operation)
            {
                case "Trace":
                {
                    Logger.Trace("This is a sample trace message");
                    break;
                }
                case "Debug":
                {
                    Logger.Debug("This is a sample debug message");
                    break;
                }
                case "Info":
                {
                    Logger.Info("This is a sample info message");
                    break;
                }
                case "Warn":
                {
                    Logger.Warn("This is a sample warn message");
                    break;
                }
                case "Error":
                {
                    Logger.Error("This is a sample error message");
                    break;
                }
                case "Fatal":
                {
                    Logger.Fatal("This is a sample fatal message");
                    break;
                }
                case "FromFile":
                {
                    FromFile();
                    break;
                }
                case "Huge":
                {
                    Huge();
                    break;
                }
                case "Multiple":
                {
                    Multiple();
                    break;
                }
                case "Continuous":
                {
                    Continuous();
                    break;
                }
                case "Parallel":
                {
                    Parallel();
                    break;
                }
                case "StartSyslogServer":
                {
                    StartSyslogServer();
                    break;
                }
                case "StopSyslogServer":
                {
                    StopSyslogServer();
                    break;
                }
                default:
                {
                    throw new InvalidOperationException($"Operation '{operation}' is not supported");
                }
            }
        }

        private void FromFile()
        {
            var messagesFromFileLogLevel = settings("MessagesFromFileLogLevel");
            var messagesFromFileFilePath = settings("MessagesFromFileFilePath");
            var logLevel = messagesFromFileLogLevel == null ? LogLevel.Trace : LogLevel.FromString(messagesFromFileLogLevel);
            InternalLogger.Debug($"From file log level: {logLevel.Name}");

            var fileNotFound = !File.Exists(messagesFromFileFilePath);
            if (fileNotFound)
            {
                InternalLogger.Debug($"From file input file '{messagesFromFileFilePath}' does not exist");
                return;
            }

            var messages = File.ReadAllLines(messagesFromFileFilePath).ToList();
            messages.ForEach(m => Logger.Log(logLevel, m));
        }

        private static void Huge()
        {
            Task.Factory.StartNew(() =>
            {
                for (var i = 0; i < 101202; i++)
                {
                    Logger.Warn(HugeMessage);
                    Thread.Sleep(10);
                }
            });
        }

        private static void Multiple()
        {
            const string paddedNumber = "D6";
            System.Threading.Tasks.Parallel.For(1, 101, i => Logger.Log(LogLevel.Trace, i.ToString(paddedNumber)));
            System.Threading.Tasks.Parallel.For(101, 201, i => Logger.Log(LogLevel.Debug, i.ToString(paddedNumber)));
            System.Threading.Tasks.Parallel.For(201, 301, i => Logger.Log(LogLevel.Info, i.ToString(paddedNumber)));
            System.Threading.Tasks.Parallel.For(301, 401, i => Logger.Log(LogLevel.Warn, i.ToString(paddedNumber)));
            System.Threading.Tasks.Parallel.For(401, 501, i => Logger.Log(LogLevel.Error, i.ToString(paddedNumber)));
            System.Threading.Tasks.Parallel.For(501, 601, i => Logger.Log(LogLevel.Fatal, i.ToString(paddedNumber)));
        }

        private static void Continuous()
        {
            Task.Factory.StartNew(() =>
            {
                for (var i = 0; i < 101202; i++)
                {
                    Logger.Warn(i);
                    Thread.Sleep(10);
                }
            });
        }

        private static void Parallel()
        {
            Task.Factory.StartNew(() =>
            {
                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
                System.Threading.Tasks.Parallel.For(1, 404505, parallelOptions, i => Logger.Warn(i));
            });
        }

        private void StartSyslogServer()
        {
            toggleSyslogServer(true, syslogServer);
        }

        private void StopSyslogServer()
        {
            toggleSyslogServer(false, syslogServer);
        }
    }
}