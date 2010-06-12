using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TestApp
{
    public partial class Form1 : Form
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public Form1()
        {
            InitializeComponent();
        }

        private void ButtonLog_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            switch (btn.Name)
            {
                case "buttonTrace":
                    logger.Trace("This is a sample trace message");
                    break;
                case "buttonDebug":
                    logger.Debug("This is a sample debug message");
                    break;
                case "buttonInfo":
                    logger.Info("This is a sample info message");
                    break;
                case "buttonWarn":
                    logger.Warn("This is a sample warn message");
                    break;
                case "buttonError":
                    logger.Error("This is a sample error message");
                    break;
                case "buttonFatal":
                    logger.Fatal("This is a sample fatal message");
                    break;
            }
        }
    }
}
