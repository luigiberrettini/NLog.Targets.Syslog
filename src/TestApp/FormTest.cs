using NLog;
using NLog.Common;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestApp
{
    public partial class FormTest : Form
    {
        private static readonly Logger Logger;

        static FormTest()
        {
            InternalLogger.LogLevel = LogLevel.Trace;
            InternalLogger.LogToTrace = true;
            Logger = LogManager.GetCurrentClassLogger();
        }

        public FormTest()
        {
            InitializeComponent();
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
                case "buttonMultiple":
                {
                    const string paddedNumber = "D6";
                    Parallel.For(1, 101, i => Logger.Log(LogLevel.Trace, i.ToString(paddedNumber)));
                    Parallel.For(101, 201, i => Logger.Log(LogLevel.Debug, i.ToString(paddedNumber)));
                    Parallel.For(201, 301, i => Logger.Log(LogLevel.Info, i.ToString(paddedNumber)));
                    Parallel.For(301, 401, i => Logger.Log(LogLevel.Warn, i.ToString(paddedNumber)));
                    Parallel.For(401, 501, i => Logger.Log(LogLevel.Error, i.ToString(paddedNumber)));
                    Parallel.For(501, 601, i => Logger.Log(LogLevel.Fatal, i.ToString(paddedNumber)));
                    break;
                }
                default:
                {
                    throw new InvalidOperationException($"Button name '{btn.Name}' is not supported");
                }
            }
        }
    }
}