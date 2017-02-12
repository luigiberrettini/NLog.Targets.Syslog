[![NuGet](https://img.shields.io/nuget/v/NLog.Targets.Syslog.svg)](https://www.nuget.org/packages/NLog.Targets.Syslog/)

Syslog target for NLog
======================
**NLog Syslog** is a custom target for **NLog**: [http://nlog-project.org](http://nlog-project.org/).

It can be used with version 4.4.2 and later of NLog and allows to send logging messages to a Syslog server.

**Notice**

Support is provided for the latest major version, but development will be based only on the latest version.



## License
NLog Syslog is open source software, licensed under the terms of BSD license.

Please see the [LICENSE file](LICENSE) for further information.



## Configuration
To use NLog Syslog simply download the [NLog.Targets.Syslog NuGet package](https://www.nuget.org/packages/NLog.Targets.Syslog/), then use as you would any NLog target.

Since this targets take advantage of [.NET Task Parallel Library](https://msdn.microsoft.com/en-us/library/dd460717.aspx) to work in an asynchronous and concurrent way, there is no need to use NLog **AsyncWrapper**.

Below is a sample NLog.config file:

```xml
<?xml version="1.0" encoding="utf-8"?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <targets>
    <target type="Syslog" name="cee-udp">
      <layout type="SimpleLayout" text="@cee: {&quot;message&quot;: &quot;${message}&quot;}" />
      <messageCreation>
        <facility>Local4</facility>
        <rfc>Rfc5424</rfc>
        <rfc5424>
          <hostname type="SimpleLayout" text="${machinename}" />
          <appName type="SimpleLayout" text="DAEMON.MyAppName" />
          <procId type="SimpleLayout" text="${processid}" />
          <msgId type="SimpleLayout" text="${threadid}" />
          <disableBom>true</disableBom>
        </rfc5424>
      </messageCreation>
    </target>
  </targets>
  <rules>
    <logger name="*" minlevel="Debug" writeTo="cee-udp" />
  </rules>
</nlog>
```


### Options
This NLog target supports the standard NLog [layout](https://github.com/NLog/NLog/wiki/Layouts)
directive to modify the log message body (Syslog packet elements are not affected).

It provides default values for all settings which can be overridden by means of the XML configuration, as shown above.
A more detailed example is included in the [test application](./src/TestApp/NLog.config).


#### Enforcement element
* `throttling` - settings related to message throttling:
  * `limit` - the number of log entries, waiting to be processed, that triggers throttling (default: `0`)
  * `strategy` - `None` / `DiscardOnFixedTimeout` / `DiscardOnPercentageTimeout` / `Discard` / `DeferForFixedTime` / `DeferForPercentageTime` / `Block`
 (default: `None`)
  * `delay` - the milliseconds/percentage delay for a `DiscardOnFixedTimeout` / `DiscardOnPercentageTimeout` / `Defer` throttling strategy (default: `0`)
* `messageProcessors` - the amount of parallel message processors (default: `1`; `0` means `Environment.ProcessorCount`)
* `splitOnNewLine` - whether or not to split each log entry by newlines and send each line separately (default: `false`)
* `transliterate` - `false` or `true` to trasliterate strings from Unicode to ASCII when the RFC allows only ASCII characters for a fields (default: `false`)
* `replaceInvalidCharacters` - `false` or `true` to replace invalid values usually with a question mark (default: `false`)
* `truncateFieldsToMaxLength` - `false` or `true` to truncate fields to the length specified in the RFC (default: `false`)
* `truncateMessageTo` - a number specifying the max lenght allowed for the whole message (default: `0` i.e. do not truncate)

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

<sup>1</sup> IP payload - max IP header - max protocol header \
<sup>2</sup> Using jumbograms (limited by Int32.MaxValue = 2147483647, i.e. the maximum size for an array)


#### Message creation element
* `facility` - facility name (default: `Local1`)
* `rfc` - `rfc3164` or `rfc5424` (default: `rfc5424`)
* `rfc3164` - settings related to RFC 3164:
  * `hostname` ([Layout](http://github.com/NLog/NLog/wiki/Layouts)) - the HOSTNAME part (default: the hostname of the computer that is creating the message)
  * `tag` ([Layout](http://github.com/NLog/NLog/wiki/Layouts)) - the TAG part (default: the name of the assembly that is creating the message)
* `rfc5424` - settings related to RFC 5424:
  * `hostname` ([Layout](http://github.com/NLog/NLog/wiki/Layouts)) - the HOSTNAME field of the HEADER part (default: the hostname of the computer that is creating the message)
  * `appName` ([Layout](http://github.com/NLog/NLog/wiki/Layouts)) - the APPNAME field of the HEADER part (default: the name of the assembly that is creating the message)
  * `procId` ([Layout](http://github.com/NLog/NLog/wiki/Layouts)) - the PROCID field of the HEADER part (default: `-`)
  * `msgId` ([Layout](http://github.com/NLog/NLog/wiki/Layouts)) - the MSGID field of the HEADER part (default: `-`)
  * `structuredData` - the STRUCTURED-DATA part containing the SD-ELEMENTs each composed by an SD-ID ([Layout](http://github.com/NLog/NLog/wiki/Layouts))
    and optional SD-PARAM fields, i.e. the PARAM-NAME ([Layout](http://github.com/NLog/NLog/wiki/Layouts)) and
    PARAM-VALUE ([Layout](http://github.com/NLog/NLog/wiki/Layouts)) fields (default: `-`).<br />
    The fromEventProperties attribute allows to use [log event properties data](http://github.com/NLog/NLog/wiki/EventProperties-Layout-Renderer)
    enabling different STRUCTURED-DATA for each log message
  * `disableBom` - `true` or `false` to handle RSyslog [issue 284](http://github.com/rsyslog/rsyslog/issues/284) (default: `false`)

#### Message send element
* `protocol` - `udp` or `tcp` (default: `udp`)
* `udp` - settings related to UDP:
  * `server` - IP or hostname of the Syslog server (default: `127.0.0.1`)
  * `port` - port the Syslog server is listening on (default: `514`)
* `tcp` - settings related to TCP:
  * `server` - IP or hostname of the Syslog server (default: `127.0.0.1`)
  * `port` - port the Syslog server is listening on (default: `514`)
  * `reconnectInterval` - the time interval, in milliseconds, after which a connection is retried (default: `500`)
  * `keepAlive` - settings related to keep-alive:
    * `enabled` - whether to use keep-alive or not (default: `true`)
    * `timeout` - the timeout, in milliseconds, with no activity until the first keep-alive packet is sent (default: `100`)
    * `interval` - the interval, in milliseconds, between when successive keep-alive packets are sent if no acknowledgement is received (default: `100`)
  * `connectionCheckTimeout` - the time, in microseconds, to wait for a response when checking the connection status (default: `100`; `0` means the only check performed is `TcpClient.IsConnected`)
  * `useTls` - `false` or `true` (default: `true`)
  * `framing` - `nonTransparent` or `octectCounting` (default: `octectCounting`)
  * `dataChunkSize` - the size of chunks, in bytes, in which data is split to be sent over the wire (default: `4096`)



<br />
<br />



# Test bench
 1. `[HOST]` Download VirtualBox and Vagrant and install them
 2. `[HOST]` Create a Vagrantfile

    ```ruby
    Vagrant.configure("2") do |config|
      config.vm.box = "ubuntu/trusty64"
      config.ssh.host = "127.0.0.1"
      config.ssh.username = "vagrant"
      config.ssh.password = "vagrant"
      config.vm.network :forwarded_port, id: 'ssh', guest: 22, host: 2222, auto_correct: false
      config.vm.network :forwarded_port, guest: 514, host: 1514, protocol: "tcp", auto_correct: false
      config.vm.network :forwarded_port, guest: 514, host: 1514, protocol: "udp", auto_correct: false
    end
    ```

 3. `[HOST]` Start the VM

    ```shell
    vagrant up
    ```

 4. `[HOST]` Connect to the VM with SSH on port 2222
 5. `[GUEST]` Switch to the root user

    ```shell
    su
    ```

 6. `[GUEST]` Uncomment the following `/etc/rsyslog.conf` lines:

    ```
    #$ModLoad imudp
    #$UDPServerRun 514
    ```
    ```
    #$ModLoad imtcp
    #$InputTCPServerRun 514
    ```

 7. `[GUEST]` Add the following `/etc/rsyslog.d/50-default.conf` line under the `user.*` one (prefixing a path with the minus sign omits flushing after every log event)

    ```
    local4.*                        /var/log/local4.log
    ```

 8. `[GUEST]` Restart Syslog service

    ```shell
    service rsyslog restart
    ```

 9. `[HOST]` Restart the VM

    ```shell
    vagrant reload
    ```

11. `[GUEST]` Make sure RSyslog is running

    ```shell
    ps -A | grep rsyslog
    ```

12. `[GUEST]` Check RSyslog configuration

    ```shell
    rsyslogd -N1
    ```

13. `[GUEST]` Check Linux system log for RSyslog errors

    ```shell
    cat /var/log/syslog | grep rsyslog
    ```

14. `[GUEST]` Perform a local test

    ```shell
    logger --server 127.0.0.1 --port 514 --priority local4.error "TCP local test"
    logger --server 127.0.0.1 --port 514 --priority local4.warning --udp "UDP local test"
    tail -3 /var/log/syslog
    tail -3 /var/log/local4.log
    ```

15. `[GUEST]` Prepare for a remote test

    ```shell
    tail -f /var/log/syslog
    ```

    OR

    ```shell
    tcpdump port 514 -vv
    ```

16. `[HOST]` Perform a remote test

    ```shell
    telnet 127.0.0.1 1514
    ```

17. `[HOST]` Perform a remote test with the NLog target (configuring it to use the Local4 facility)



<br />
<br />



# Syslog message format
Messages are built using the format defined in
[RFC 3164](http://tools.ietf.org/html/rfc3164) or
[RFC 5424](http://tools.ietf.org/html/rfc5424).
They are then sent using one of the protocols defined in
[RFC 5426](http://tools.ietf.org/html/rfc5426) or
[RFC 6587](http://tools.ietf.org/html/rfc6587) or
[RFC 5425](http://tools.ietf.org/html/rfc5425).


### RFC 3164
There are no set requirements on the contents of the Syslog message: the payload of any Syslog message must be considered to be a valid Syslog message.
It is, however, recommended for the Syslog message to have all the parts described here.

#### Conventions
* `SPACE`: the ASCII value `dec 32` / `hex 20`
* `PRINTUSASCII`: ASCII values in the range `dec [33, 126]` / `hex [21, 7E]`

#### Message parts
A Syslog message is at least 1 and at most 1024 characters long and the only allowed characters are `SPACE` or `PRINTUSASCII`

```
SYSLOG MESSAGE = PRI HEADER SPACE MSG

PRI = < PRIVAL >
    PRIVAL = FACILITY * 8 + SEVERITY 
        FACILITY
            A number between 0 and 23
        SEVERITY
            A number between 0 and 7

HEADER = TIMESTAMP space HOSTNAME (only SPACE or PRINTUSASCII allowed)
    TIMESTAMP
        "Mmm dd hh:mm:ss" using a local timezone
        Space-padding in dd, zero-padding in hh, mm and ss
    HOSTNAME
        Hostname or IPv4 address or IPv6 address of the sender machine

MSG = TAG CONTENT
    TAG
        Name of the sender program or process
        An alphanumeric string not exceeding 32 characters
    CONTENT
        Detailed information of the event
        A non-alphanumeric character followed by SPACE or PRINTUSASCII characters
```

#### Examples

* `<34>Oct 11 00:14:05 mymachine su: 'su root' failed for lonvick on /dev/pts/8`
* `<13>Feb  5 17:32:18 10.0.0.99 myTag Use the BFG!`


### RFC 5424


#### Conventions
* `(section)`: brackets are used to indicate that a section is optional
* `NILVALUE`: the hyphen i.e. ASCII value dec 45 / hex 2D
* `SPACE`: the ASCII value `dec 32` / `hex 20`
* `PRINTUSASCII`: ASCII values in the range `dec [33, 126]` / `hex [21, 7E]`
* `SAFEPRINTUSASCII`: PRINTUSASCII except `=`, `]`, `"`

#### Message parts
A Syslog message is at most 480 to 2048 or more bytes

This is the detail of the format:
```
SYSLOG MESSAGE = HEADER SPACE STRUCTURED-DATA (SPACE MSG)

HEADER = PRI VERSION SPACE TIMESTAMP SPACE HOSTNAME SPACE APPNAME SPACE PROCID SPACE MSGID
    PRI = < PRIVAL >
        PRIVAL = FACILITY * 8 + SEVERITY
            FACILITY
                A number between 0 and 23
            SEVERITY
                A number between 0 and 7
        VERSION
            A nonzero digit followed by 0 to 2 digits (current version is 1)
        TIMESTAMP
            NILVALUE or RFC3339 timestamp with an optional 1 to 6 digits second fraction part
        HOSTNAME
            NILVALUE or 1 to 255 PRINTUSASCII
            The FQDN or IPv4 address or IPv6 address or hostname of the sender machine
        APPNAME
            NILVALUE or 1 to 48 PRINTUSASCII
            The device or application sending the Syslog message 
        PROCID
            NILVALUE or 1 to 128 PRINTUSASCII
            A change indicates a discontinuity in Syslog reporting
            Often the process name or id or an identifier of the group the Syslog message belongs to
        MSGID
            NILVALUE or 1 to 32 PRINTUSASCII
            The type of message that should be the same for events with the same semantics

STRUCTURED-DATA = NILVALUE or one or more SD-ELEMENT
    SD-ELEMENT = [ SD-ID (one or more SPACE SD-PARAM) ]
        SD-ID
            At most 32 SAFEPRINTUSASCII specifying a unique identifier within STRUCTUREDDATA
            The identifier can be a CUSTOMID or IANAID:
                CUSTOMID = NAME @ PEN
                    NAME
                        SAFEPRINTUSASCII except @
                    PEN
                        A private enterprise number
                        Digits or digits separated by periods
                IANAID = timeQuality or origin or meta
                    timeQuality
                        Parameters are tzKnown, isSynced, syncAccuracy
                    origin
                        Parameters are ip, enterpriseId, software, swVersion
                    meta
                        Parameters are sequenceId, sysUpTime, language
        SD-PARAM = PARAM-NAME = " PARAM-VALUE "
            PARAM-NAME
                1 to 32 SAFEPRINTUSASCII
            PARAM-VALUE
                UTF8 STRING with ", \ and ] escaped as \", \\ and \]

MSG = MSGANY or MSGUTF8
    MSGANY
        Zero or more bytes
    MSGUTF8 = BOM UTF8STRING
        BOM
            The hex value EFBBBF
        UTF8STRING
            A UTF8 compliant string
```

#### Examples

* `<34>1 2003-10-11T22:14:15.003Z mymachine.example.com su - ID47 - BOM'su root' failed for lonvick on /dev/pts/8`
* `<165>1 2003-08-24T05:14:15.000003-07:00 192.0.2.1 myproc 8710 - - %% It's time to make the do-nuts.`
* `<165>1 2003-10-11T22:14:15.003Z mymachine.example.com evntslog - ID47 [exampleSDID@32473 iut="3" eventSource="Application" eventID="1011"] BOMAn application event log entry...`
* `<165>1 2003-10-11T22:14:15.003Z mymachine.example.com evntslog - ID47 [exampleSDID@32473 iut="3" eventSource="Application" eventID="1011"][examplePriority@32473 class="high"]`
