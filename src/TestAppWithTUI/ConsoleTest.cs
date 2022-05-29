// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FakeSyslogServer;
using Microsoft.Extensions.Configuration;

namespace TestAppWithTui
{
    public class ConsoleTest : IDisposable
    {
        private readonly IConfigurationRoot settings;
        private readonly Dictionary<char, string> availableOperations;
        private readonly TestAppHelper testAppHelper;
        private FileStream udpFileStream;
        private StreamWriter udpStreamWriter;
        private FileStream tcpFileStream;
        private StreamWriter tcpStreamWriter;

        public ConsoleTest()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json");
            settings = builder.Build();
            availableOperations = new Dictionary<char, string>
            {
                { 'T', "Trace" },
                { 'D', "Debug" },
                { 'I', "Info" },
                { 'W', "Warn" },
                { 'E', "Error" },
                { 'F', "Fatal" },
                { 'R', "FromFile" },
                { 'H', "Huge" },
                { 'M', "Multiple" },
                { 'C', "Continuous" },
                { 'P', "Parallel" },
                { 'S', "StartSyslogServer" },
                { 'Q', "Quit" }
            };
            testAppHelper = new TestAppHelper(key => settings[key], ToggleSyslogServer);
        }

        public string PromptUserForOperationChoice()
        {
            Console.WriteLine("Choose an operation:");
            availableOperations
                .Select(x => $"{x.Key} => {x.Value}")
                .ToList()
                .ForEach(Console.WriteLine);

            var chosenOperation = Char.ToUpper(Console.ReadKey().KeyChar);
            Console.WriteLine();
            return availableOperations[chosenOperation];
        }

        public void PerformOperation(string operation)
        {
            testAppHelper.PerformSelectedOperation(operation);
        }

        public void Dispose()
        {
            testAppHelper.Dispose();
            DisposeStreams();
            GC.SuppressFinalize(this);
        }

        private void ToggleSyslogServer(bool start, SyslogServer syslogServer)
        {
            if (start)
            {
                availableOperations['S'] = "StopSyslogServer";
                InitStreams();
                syslogServer.Start(OnReceivedString, OnException);
            }
            else
            {
                availableOperations['S'] = "StartSyslogServer";
                syslogServer.Stop();
                DisposeStreams();
            }
        }

        private void InitStreams()
        {
            udpFileStream = new FileStream(settings["udpLogEntriesFilePath"], FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            udpFileStream.SetLength(0);
            udpStreamWriter = new StreamWriter(udpFileStream);

            tcpFileStream = new FileStream(settings["tcpLogEntriesFilePath"], FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            tcpFileStream.SetLength(0);
            tcpStreamWriter = new StreamWriter(tcpFileStream);
        }

        private void OnReceivedString(int protocolType, string receivedString)
        {
            Trace.WriteLine(receivedString);

            var file = protocolType == SyslogServer.UdpProtocolHashCode ? udpStreamWriter : tcpStreamWriter;
            file?.WriteLine(receivedString);
            file?.Flush();
        }

        private void OnException(Task task)
        {
            Trace.WriteLine(this, task.Exception?.GetBaseException().ToString());
        }

        private void DisposeStreams()
        {
            udpStreamWriter?.Close();
            udpStreamWriter = null;
            udpFileStream?.Close();
            udpFileStream = null;
            tcpStreamWriter?.Close();
            tcpStreamWriter = null;
            tcpFileStream?.Close();
            tcpFileStream = null;
        }
    }
}