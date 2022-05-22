# Configuration

NLog Syslog target takes advantage of the [.NET Task Parallel Library](https://msdn.microsoft.com/en-us/library/dd460717.aspx) to work in an asynchronous and concurrent way, therefore the NLog **AsyncWrapper** should not be used.

The standard NLog [layout](https://github.com/NLog/NLog/wiki/Layouts) directive is used to modify the log message body: Syslog packet elements are not affected.

Default configuration values for all settings are provided and can be overridden by means of XML/programmatic configuration.

There are three configuration sections:
 - enforcement
 - message creation
 - message send



## Sample configuration

Below is a sample NLog.config file:

```xml
<?xml version="1.0" encoding="utf-8"?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      xmlns:sl="http://www.nlog-project.org/schemas/NLog.Targets.Syslog.xsd">
  <extensions>
    <add assembly="NLog.Targets.Syslog"/>
  </extensions>
  <targets>
    <target xsi:type="Syslog" name="cee-udp">
      <sl:layout xsi:type="SimpleLayout" text="@cee: {&quot;message&quot;: &quot;${message}&quot;}" />
      <sl:messageCreation>
        <sl:facility>Local4</sl:facility>
        <sl:rfc>Rfc5424</sl:rfc>
        <sl:rfc5424>
          <sl:hostname xsi:type="SimpleLayout" text="${machinename}" />
          <sl:appName xsi:type="SimpleLayout" text="DAEMON.MyAppName" />
          <sl:procId xsi:type="SimpleLayout" text="${processid}" />
          <sl:msgId xsi:type="SimpleLayout" text="${threadid}" />
          <sl:disableBom>true</sl:disableBom>
        </sl:rfc5424>
      </sl:messageCreation>
    </target>
  </targets>
  <rules>
    <logger name="*" minlevel="Debug" writeTo="cee-udp" />
  </rules>
</nlog>
```

A more detailed example is [included](../src/TestAppWithGUI/NLog.config) in the test application.



## Enforcement settings

 - `throttling` - settings related to message throttling:
    - `limit` - the number of log entries, waiting to be processed, that triggers throttling (default: `65536`; `0` means no limit)
    - `strategy` - `None` / `DiscardOnFixedTimeout` / `DiscardOnPercentageTimeout` / `Discard` / `DeferForFixedTime` / `DeferForPercentageTime` / `Block` (default: `Discard`)
    - `delay` - the milliseconds/percentage delay for a `DiscardOnFixedTimeout` / `DiscardOnPercentageTimeout` / `Defer` throttling strategy (default: `0`)
 - `messageProcessors` - the amount of parallel message processors (default: `1`; `0` means `Environment.ProcessorCount`)
 - `splitOnNewLine` - whether or not to split each log entry by newlines and send each line separately (default: `false`)
 - `transliterate` - `false` or `true` to transliterate strings from Unicode to ASCII when the RFC allows only ASCII characters for a field (default: `false`)
 - `replaceInvalidCharacters` - `false` or `true` to replace invalid values usually with a question mark (default: `false`)
 - `truncateFieldsToMaxLength` - `false` or `true` to truncate fields to the length specified in the RFC (default: `false`)
 - `truncateMessageTo` - a number specifying the max length allowed for the whole message (default: `0` i.e. do not truncate)

The maximum length of a message is detailed in many RFCs that can be summarized as follow:

|                       |  MUST be supported  |  SHOULD be supported  |  MUST NOT exceed
| :-------------------: | :-----------------: | :-------------------: | :--------------------------------------------------:
|  RFC 3164 (UDP)       |  1024 B             |  1024 B               |  1024 B
|  RFC 6587 (TCP)       |  1024 B             |  1024 B               |  1024 B
|  RFC 5424 (TCP/UDP)   |   480 B             |  2048 B               |  -
|  RFC 5426 (UDP/IPv4)  |   480 B             |  2048 B               |  65535      - 60 -  8 B <sup>1</sup>
|  RFC 5426 (UDP/IPv6)  |  1180 B             |  2048 B               |  65535      - 40 -  8 B <sup>1</sup>
|  RFC 5426 (UDP/IPv6)  |  1180 B             |  2048 B               |  (2^32 - 1) - 40 -  8 B <sup>1</sup> <sup>2</sup>
|  RFC 5425 (TLS/IPv4)  |  2048 B             |  8192 B               |  65535      - 60 - 60 B <sup>1</sup>
|  RFC 5425 (TLS/IPv6)  |  2048 B             |  8192 B               |  65535      - 40 - 60 B <sup>1</sup>
|  RFC 5425 (TLS/IPv6)  |  2048 B             |  8192 B               |  (2^32 - 1) - 40 - 60 B <sup>1</sup> <sup>2</sup>

<sup>1</sup> IP payload - max IP header - max protocol header

<sup>2</sup> Using jumbograms (limited by Int32.MaxValue = 2147483647, i.e. the maximum size for an array)



## Message creation settings

 - `facility` - facility name (default: `Local1`)
 - `perLogLevelSeverity` - the severity to be used for each log level:
    - `fatal` - `Emergency` / `Alert` / `Critical` / `Error` / `Warning` / `Notice` / `Informational` / `Debug` (default: `Emergency`)
    - `error` - `Emergency` / `Alert` / `Critical` / `Error` / `Warning` / `Notice` / `Informational` / `Debug` (default: `Error`)
    - `warn` - `Emergency` / `Alert` / `Critical` / `Error` / `Warning` / `Notice` / `Informational` / `Debug` (default: `Warning`)
    - `info` - `Emergency` / `Alert` / `Critical` / `Error` / `Warning` / `Notice` / `Informational` / `Debug` (default: `Informational`)
    - `debug` - `Emergency` / `Alert` / `Critical` / `Error` / `Warning` / `Notice` / `Informational` / `Debug` (default: `Debug`)
    - `trace` - `Emergency` / `Alert` / `Critical` / `Error` / `Warning` / `Notice` / `Informational` / `Debug` (default: `Notice`)
 - `rfc` - `rfc3164` or `rfc5424` (default: `rfc5424`)
 - `rfc3164` - settings related to RFC 3164:
    - `outputPri` - `true` or `false` to output or not the PRI part (default: true, used for custom messages)
    - `outputHeader` - `true` or `false` to output or not the HEADER part (default: true, used for custom messages)
    - `outputSpaceBeforeMsg` - `true` or `false` to output or not the space before the MSG part (default: true, used for custom messages)
    - `hostname` ([Layout](https://github.com/NLog/NLog/wiki/Layouts)) - the HOSTNAME field of the HEADER part (default: the hostname of the computer that is creating the message)
    - `tag` ([Layout](https://github.com/NLog/NLog/wiki/Layouts)) - the TAG field of the MSG part (default: the name of the assembly that is creating the message)
 - `rfc5424` - settings related to RFC 5424:
    - `timestampFractionalDigits` - the number of fractional digits for the TIMESTAMP field of the HEADER part (default: 6, max: 16 as per ISO 8601 but since .NET is limited to 7 the other digits will be zeroed)
    - `hostname` ([Layout](https://github.com/NLog/NLog/wiki/Layouts)) - the HOSTNAME field of the HEADER part (default: the hostname of the computer that is creating the message)
    - `appName` ([Layout](https://github.com/NLog/NLog/wiki/Layouts)) - the APPNAME field of the HEADER part (default: the name of the assembly that is creating the message)
    - `procId` ([Layout](https://github.com/NLog/NLog/wiki/Layouts)) - the PROCID field of the HEADER part (default: `-`)
    - `msgId` ([Layout](https://github.com/NLog/NLog/wiki/Layouts)) - the MSGID field of the HEADER part (default: `-`)
    - `structuredData` - the STRUCTURED-DATA part containing the SD-ELEMENTs each composed by an SD-ID ([Layout](https://github.com/NLog/NLog/wiki/Layouts)) and optional SD-PARAM fields, i.e. the PARAM-NAME ([Layout](https://github.com/NLog/NLog/wiki/Layouts)) and PARAM-VALUE ([Layout](https://github.com/NLog/NLog/wiki/Layouts)) fields (default: `-`).<br />
      The fromEventProperties attribute allows to use [log event properties data](https://github.com/NLog/NLog/wiki/EventProperties-Layout-Renderer) enabling different STRUCTURED-DATA for each log message
    - `disableBom` - `true` or `false` to handle RSyslog [issue 284](https://github.com/rsyslog/rsyslog/issues/284) (default: `false`)



## Message send settings

 - `protocol` - `udp` or `tcp` (default: `udp`)
 - `udp` - settings related to UDP:
    - `server` - IP or hostname of the Syslog server (default: `127.0.0.1`)
    - `port` - port the Syslog server is listening on (default: `514`)
    - `reconnectInterval` - the time interval, in milliseconds, after which a connection is retried (default: `500`)
 - `tcp` - settings related to TCP:
    - `server` - IP or hostname of the Syslog server (default: `127.0.0.1`)
    - `port` - port the Syslog server is listening on (default: `514`)
    - `reconnectInterval` - the time interval, in milliseconds, after which a connection is retried (default: `500`)
    - `keepAlive` - settings related to keep-alive:
       - `enabled` - whether to use keep-alive or not (default: `true`)
       - `retryCount` - the number of unacknowledged keep-alive probes to send before considering the connection dead and terminating it (default: `10`)
       - `time` - the number of seconds a connection will remain idle before the first keep-alive probe is sent (default: `5`)
       - `interval` - the number of seconds a connection will wait for a keep-alive acknowledgement before sending another keepalive probe (default: `1`)
    - `tls` - settings related to TLS:
       - `enabled` - whether to use TLS or not (TLS 1.2 only) (default `false`)
       - `useClientCertificates` - whether to use client certificates or not (default `false`)
       - `certificateStoreLocation` - the X.509 certificate store [location](https://msdn.microsoft.com/en-us/library/system.security.cryptography.x509certificates.storelocation.aspx) (default `CurrentUser`)
       - `certificateStoreName` - the X.509 certificate store [name](https://msdn.microsoft.com/en-us/library/system.security.cryptography.x509certificates.storename.aspx) (default `My`)
       - `certificateFilterType` - the [type of filter](https://msdn.microsoft.com/en-us/library/system.security.cryptography.x509certificates.x509findtype.aspx) to apply to the certificate collection (default `FindBySubjectName`)
       - `certificateFilterValue` - the value against which to filter the certificate collection
    - `framing` - `nonTransparent` or `octectCounting` (default: `octectCounting`)