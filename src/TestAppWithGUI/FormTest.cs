// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows.Forms;
using FakeSyslogServer;
using NLog.Targets.Syslog.Settings;

namespace TestAppWithGui
{
    public partial class FormTest : Form
    {
        private readonly Action<int, string> receivedStringAction;
        private readonly Action<Task> exceptionAction;
        private readonly TestAppHelper testAppHelper;

        public FormTest()
        {
            InitializeComponent();
            receivedStringAction = OnReceivedStringAction();
            exceptionAction = OnExceptionAction();
            testAppHelper = new TestAppHelper(key => ConfigurationManager.AppSettings[key], ToggleSyslogServer);
        }

        private void ButtonLogClick(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var operation = btn.Name.Replace("button", string.Empty);
            testAppHelper.PerformSelectedOperation(operation);
        }

        private void ToggleSyslogServer(bool start, SyslogServer syslogServer)
        {
            buttonStartStopSyslogServer.Enabled = false;

            var operation = start ? "Stop" : "Start";
            buttonStartStopSyslogServer.Name = $"button{operation}SyslogServer";
            buttonStartStopSyslogServer.Text = $@"{operation} Syslog Server";

            if (start)
                syslogServer.Start(receivedStringAction, exceptionAction);
            else
                syslogServer.Stop();

            Action enableButton = () => buttonStartStopSyslogServer.Enabled = true;
            Task.Delay(500).ContinueWith(_ => Invoke(enableButton));
        }

        private Action<int, string> OnReceivedStringAction()
        {
            Action<int, string> appendStringAction = (protocolType, recString) =>
            {
                var textBox = protocolType == SyslogServer.UdpProtocolHashCode ? udpTextBox : tcpTextBox;
                textBox.AppendText(recString);
                textBox.AppendText(Environment.NewLine);
            };
            return (protocolType, recString) => Invoke(appendStringAction, protocolType, recString);
        }

        private Action<Task> OnExceptionAction()
        {
            Action<Task> msgBoxAction = task => MessageBox.Show(this, task.Exception?.GetBaseException().ToString());
            return task => Invoke(msgBoxAction, task);
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            testAppHelper.Dispose();
        }
    }
}