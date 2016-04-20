using NLog;
using NLog.Common;
using System;
using System.Windows.Forms;

namespace TestApp
{
    public partial class Form1 : Form
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Form1()
        {
            InternalLogger.LogLevel = LogLevel.Trace;
            InternalLogger.LogToTrace = true;
            InitializeComponent();
        }

        private void ButtonLogClick(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            switch (btn.Name)
            {
                case "buttonTrace":
                    Logger.Trace("This is a sample trace message");
                    break;
                case "buttonDebug":
                    Logger.Debug("This is a sample debug message");
                    break;
                case "buttonInfo":
                    Logger.Info("This is a sample info message");
                    break;
                case "buttonWarn":
                    Logger.Warn("This is a sample warn message");
                    break;
                case "buttonError":
                    Logger.Error("This is a sample error message");
                    break;
                case "buttonFatal":
                    Logger.Fatal("This is a sample fatal message");
                    break;
                default:
                    throw new InvalidOperationException($"Button name '{btn.Name}' is not supported");
            }
        }
    }
}