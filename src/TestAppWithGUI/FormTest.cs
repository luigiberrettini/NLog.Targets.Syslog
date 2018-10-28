// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows.Forms;
using FakeSyslogServer;

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

            void EnableButton() => buttonStartStopSyslogServer.Enabled = true;
            Task.Delay(500).ContinueWith(_ => Invoke((Action) EnableButton));
        }

        private Action<int, string> OnReceivedStringAction()
        {
            void AppendStringAction(int protocolType, string recString)
            {
                var textBox = protocolType == SyslogServer.UdpProtocolHashCode ? udpTextBox : tcpTextBox;
                textBox.AppendText(recString);
                textBox.AppendText(Environment.NewLine);
            }

            return (protocolType, recString) => Invoke((Action<int, string>) AppendStringAction, protocolType, recString);
        }

        private Action<Task> OnExceptionAction()
        {
            void MsgBoxAction(Task task) => MessageBox.Show(this, task.Exception?.GetBaseException().ToString());
            return task => Invoke((Action<Task>) MsgBoxAction, task);
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            testAppHelper.Dispose();
        }
    }
}