using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NLog.Config;
using Xunit;
using NLog;
using NLog.Extensions.Logging;

namespace NLog.Targets.Syslog.Test.Dotnet
{
    public class TestLogger
    {

        public TestLogger()
        {
           
        }

        [Fact]
        public void TestLog()
        {
            ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddNLog();
            var logger = loggerFactory.CreateLogger("TestLogger");
            var config = new XmlLoggingConfiguration("Nlog.config");
            LogManager.Configuration = config;
            Assert.True(config.AllTargets.Count>0);
            logger.LogTrace("This is a sample trace message");
            logger.LogDebug("This is a sample debug message");
            logger.LogInformation("This is a sample info message");
            logger.LogWarning("This is a sample warn message");
            logger.LogError("This is a sample error message");
            logger.LogCritical("This is a sample fatal message");
        }
    }
}
