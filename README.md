<h1>Syslog Target for NLog</h1>

<b>NLog Syslog</b> is a custom target for <a href="http://nlog-project.org/">NLog</a> version 2.0 allowing you to send logging messages to a UNIX-style Syslog server.

To use NLog Syslog, you simply wire it up as an extension in the NLog.config file and place the NLog.Targets.Syslog.dll in the same location as the NLog.dll & NLog.config files. Then use as you would any NLog target. Below is a sample NLog.config file:

<pre>&lt;?xml version=<span style="color: #008080; ">"1.0"</span> encoding=<span style="color: #008080; ">"utf-8"</span> ?&gt;
&lt;nlog xmlns=<span style="color: #008080; ">"http://www.nlog-project.org/schemas/NLog.xsd"</span>
      xmlns:xsi=<span style="color: #008080; ">"http://www.w3.org/2001/XMLSchema-instance"</span>&gt;

    &lt;extensions&gt;
        &lt;add assembly=<span style="color: #008080; ">"NLog.Targets.Syslog"</span> <span style="color: Navy; ">/</span>&gt;
    &lt;/extensions&gt;
    
    &lt;targets&gt;
        &lt;target name=<span style="color: #008080; ">"syslog"</span> type=<span style="color: #008080; ">"Syslog"</span> syslogserver=<span style="color: #008080; ">""</span> port=<span style="color: #008080; ">""</span> facility=<span style="color: #008080; ">""</span><span style="color: Navy; ">/</span>&gt;
    &lt;/targets&gt;

    &lt;rules&gt;
        &lt;logger name=<span style="color: #008080; ">"*"</span> minLevel=<span style="color: #008080; ">"Trace"</span> appendTo=<span style="color: #008080; ">"syslog"</span><span style="color: Navy; ">/</span>&gt;
    &lt;/rules&gt;

&lt;/nlog&gt;</pre>

See more about NLog at: <a href="http://nlog-project.org/">http://nlog-project.org/</a>