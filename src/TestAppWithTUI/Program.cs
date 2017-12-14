// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.IO;
using System.Reflection;

namespace TestAppWithTui
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // http://github.com/dotnet/project-system/issues/2239
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

            var consoleTest = new ConsoleTest();
            while (true)
            {
                try
                {
                    var choice = consoleTest.PromptUserForOperationChoice();
                    if (choice == "Quit")
                        return;
                    consoleTest.PerformOperation(choice);
                }
                catch
                {
                    Console.WriteLine("Invalid choice");
                }
                Console.WriteLine();
            }
        }
    }
}