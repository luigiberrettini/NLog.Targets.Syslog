// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Windows.Forms;

namespace TestAppWithGui
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormTest());
        }
    }
}