// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NLog;
using TestApp;

namespace TestAppCore
{
    class Program
    {
        private static IConfiguration configuration;
        private static SyslogServer syslogServer;

        private static Logger Logger;

        static void Main(string[] args)
        {
            IConfigurationBuilder cb = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json");
            configuration = cb.Build();

            Logger = LogManager.GetCurrentClassLogger();

            bool quit = false;
            do
            {
                System.Threading.Thread.Sleep(200);
                Console.WriteLine("----------------------------------------------------------------");
                Console.WriteLine("Choose your options:");
                if (syslogServer == null)
                    Console.WriteLine(" 1 - Start SysLog Server");
                else
                    Console.WriteLine(" 1 - Stop SysLog Server");
                Console.WriteLine(" 2 - Test Trace");
                Console.WriteLine(" 3 - Test Warning");
                Console.WriteLine(" 0 - Quit");

                var input = Console.ReadKey();
                Console.WriteLine();
                switch (input.KeyChar)
                {
                    case '1': ToggleSyslogServer(); break;
                    case '2': Logger.Trace("This is a sample Trace message"); break;
                    case '3': Logger.Warn("This is a sample Warning message"); break;
                    case '0': quit = true; break;
                }
            } while (!quit);

            ToggleSyslogServer(true);
        }

        private static void ToggleSyslogServer(bool forceStop = false)
        {
            if (syslogServer == null)
            {
                if (!forceStop)
                {
                    Console.WriteLine("Starting SyslogServer...");
                    var tcpEndPoint = EndPoint("tcpIp", "tcpPort");
                    var udpEndPoint = EndPoint("udpIp", "udpPort");
                    syslogServer = new SyslogServer(udpEndPoint, tcpEndPoint);
                    syslogServer.Start(ReceivedMessage, ReceivedException);
                    System.Threading.Thread.Sleep(500);
                }
            }
            else
            {
                Console.WriteLine("Stopping SyslogServer...");
                syslogServer.Stop();
                System.Threading.Thread.Sleep(500);
                syslogServer = null;
            }
        }

        private static IPEndPoint EndPoint(string ipKey, string portKey)
        {
            var ip = IPAddress.Parse(configuration[ipKey]);
            var port = int.Parse(configuration[portKey]);
            return new IPEndPoint(ip, port);
        }

        private static void ReceivedMessage(ProtocolType protocol, string message)
        {
            Console.WriteLine(string.Concat(protocol.ToString(), "|", message));
        }

        private static void ReceivedException(Task task)
        {
            if (task != null && task.Exception != null)
            {
                Console.WriteLine(task.Exception.ToString());
            }
        }
    }
}