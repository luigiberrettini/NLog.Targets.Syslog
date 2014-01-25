Syslog Target for NLog
======================

**NLog Syslog** is a custom target for [NLog](http://nlog-project.org/) version 2.0 allowing you to send logging messages to a UNIX-style Syslog server.

To use NLog Syslog, you simply wire it up as an extension in the NLog.config file and place the NLog.Targets.Syslog.dll in the same location as the NLog.dll & NLog.config files. Then use as you would any NLog target. To use TCP as transport protocol just specify protocol="tcp" in target configuration. Below is a sample NLog.config file:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">

  <extensions>
    <add assembly="NLog.Targets.Syslog" />
  </extensions>

  <targets>
    <target name="syslog" type="Syslog" syslogserver="127.0.0.1" port="514" sender="MyProgram" facility="Local7" layout="[CustomPrefix] ${machinename} ${message}/>
  </targets>

  <rules>
    <logger name="*" minLevel="Trace" appendTo="syslog"/>
  </rules>
</nlog>
```

See more about NLog at: http://nlog-project.org
