using NLog;
using NLog.Common;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestApp
{
    public partial class FormTest : Form
    {
        private static readonly Logger Logger;
        private SyslogServer syslogServer;

        static FormTest()
        {
            InternalLogger.LogLevel = LogLevel.Warn;
            InternalLogger.LogToTrace = true;
            Logger = LogManager.GetCurrentClassLogger();
        }

        public FormTest()
        {
            InitializeComponent();
            StartSyslogServer();
        }

        private void StartSyslogServer()
        {
            var tcpEndPoint = EndPoint("tcpIp", "tcpPort");
            var udpEndPoint = EndPoint("udpIp", "udpPort");

            Action<ProtocolType, string> appendStringAction = (protocolType, recString) =>
            {
                var textBox = protocolType == ProtocolType.Udp ? udpTextBox : tcpTextBox;
                textBox.AppendText(recString);
                textBox.AppendText(Environment.NewLine);
            };
            Action<ProtocolType, string> receivedStringAction = (protocolType, recString) => Invoke(appendStringAction, protocolType, recString);

            Action<Task> msgBoxAction = task => MessageBox.Show(this, task.Exception?.GetBaseException().ToString());
            Action<Task> exceptionAction = task => Invoke(msgBoxAction, task);

            var server = new SyslogServer(udpEndPoint, tcpEndPoint);
            server.Start(receivedStringAction, exceptionAction);
        }

        private static IPEndPoint EndPoint(string ipKey, string portKey)
        {
            var ip = IPAddress.Parse(ConfigurationManager.AppSettings[ipKey]);
            var port = int.Parse(ConfigurationManager.AppSettings[portKey]);
            return new IPEndPoint(ip, port);
        }

        private void ButtonLogClick(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            switch (btn.Name)
            {
                case "buttonTrace":
                {
                    Logger.Trace("This is a sample trace message");
                    break;
                }
                case "buttonDebug":
                {
                    Logger.Debug("This is a sample debug message");
                    break;
                }
                case "buttonInfo":
                {
                    Logger.Info("This is a sample info message");
                    break;
                }
                case "buttonWarn":
                {
                    Logger.Warn("This is a sample warn message");
                    break;
                }
                case "buttonError":
                {
                    Logger.Error("This is a sample error message");
                    break;
                }
                case "buttonFatal":
                {
                    Logger.Fatal("This is a sample fatal message");
                    break;
                }
                case "buttonFromFile":
                {
                    ButtonFromFile();
                    break;
                }
                case "buttonMultiple":
                {
                    ButtonMultiple();
                    break;
                }
                case "buttonContinuous":
                {
                    ButtonContinuous();
                    break;
                }
                case "buttonParallel":
                {
                    ButtonParallel();
                    break;
                }
                default:
                {
                    throw new InvalidOperationException($"Button name '{btn.Name}' is not supported");
                }
            }
        }

        private static void ButtonFromFile()
        {
            var logLevelName = ConfigurationManager.AppSettings["MessagesFromFileLogLevel"];
            var logLevel = logLevelName == null ? LogLevel.Trace : LogLevel.FromString(logLevelName);
            InternalLogger.Debug($"From file log level: {logLevel.Name}");

            var path = ConfigurationManager.AppSettings["MessagesFromFileFilePath"];
            var fileNotFound = !File.Exists(path);
            if (fileNotFound)
            {
                InternalLogger.Debug($"From file input file '{path}' does not exist");
                return;
            }

            var messages = File.ReadAllLines(path).ToList();
            messages.ForEach(m => Logger.Log(logLevel, m));
        }

        private static void ButtonMultiple()
        {
            const string paddedNumber = "D6";
            Parallel.For(1, 101, i => Logger.Log(LogLevel.Trace, i.ToString(paddedNumber)));
            Parallel.For(101, 201, i => Logger.Log(LogLevel.Debug, i.ToString(paddedNumber)));
            Parallel.For(201, 301, i => Logger.Log(LogLevel.Info, i.ToString(paddedNumber)));
            Parallel.For(301, 401, i => Logger.Log(LogLevel.Warn, i.ToString(paddedNumber)));
            Parallel.For(401, 501, i => Logger.Log(LogLevel.Error, i.ToString(paddedNumber)));
            Parallel.For(501, 601, i => Logger.Log(LogLevel.Fatal, i.ToString(paddedNumber)));
        }

        private static void ButtonContinuous()
        {
            Task.Factory.StartNew(() =>
            {
                for (var i = 0; i < 101202; i++)
                {
                    Logger.Warn(i);
                    //Thread.Sleep(100);
                }
            });
        }

        private static void ButtonParallel()
        {
            Task.Factory.StartNew(() =>
            {
                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
                Parallel.For(1, 404505, parallelOptions, i => Logger.Warn(i));
            });
        }
    }
}