Syslog Target for NLog
======================
**NLog Syslog** is a custom target for [NLog](http://nlog-project.org/) version 4.2.2 allowing you to send logging messages to a UNIX-style Syslog server.



## Configuration
To use NLog Syslog, you simply wire it up as an extension in the NLog.config file and place the NLog.Targets.Syslog.dll in the same location as the NLog.dll & NLog.config files.
Then use as you would any NLog target.
Below is a sample NLog.config file:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">

  <extensions>
    <add assembly="NLog.Targets.Syslog" />
  </extensions>

  <targets>
    <target name="syslog" type="Syslog" syslogserver="127.0.0.1" port="514" facility="Local7" sender="MyProgram" layout="[CustomPrefix] ${machinename} ${message}" />
  </targets>

  <rules>
    <logger name="*" minLevel="Trace" appendTo="syslog"/>
  </rules>
</nlog>
```
The package is also available through NuGet. Simply search for "NLog.Targets.Syslog".


### Options
This NLog target provides default values for all configuration options.
Optionally, your configuration can override them using attributes on `target`, as shown in the example configuration above.

#### Destination
* `syslogserver`: IP or hostname (default: `127.0.0.1`)
* `port`: Port of Syslog listener (default: `514`)
* `protocol`: `udp` or `tcp` (default: `udp`)
* `ssl`: `false` or `true`; TCP only (default: `false`)
* `rfc`: Rfc compatibility for Syslog message `Rfc3164` or `Rfc5424` (default: `Rfc3164`)

#### Syslog packet elements
The following Syslog elements can be overridden for RFC 3164:

* `machinename` ([Layout](https://github.com/NLog/NLog/wiki/Layouts)): name of sending system or entity (default: machine 
  [hostname](http://msdn.microsoft.com/en-us/library/system.net.dns.gethostname(v=vs.110).aspx)).
For example, ${machinename}
* `sender` ([Layout](https://github.com/NLog/NLog/wiki/Layouts)): name of sending component or application (default: 
  [calling method](http://msdn.microsoft.com/en-us/library/system.reflection.assembly.getcallingassembly(v=vs.110).aspx)).
For example, ${logger}
* `facility`: facility name (default: `Local1`)

For example, to make logs from multiple systems use the same device identifier (rather than each system's hostname), one could set `machinename` to `app-cloud`.
The logs from different systems would all appear to be from the same single entity called `app-cloud`.

The following additional Syslog elements can be overridden for [RFC 5424](http://tools.ietf.org/html/rfc5424):

* `procid` ([Layout](https://github.com/NLog/NLog/wiki/Layouts)): [identifier](http://tools.ietf.org/html/rfc5424#section-6.2.6) (numeric or alphanumeric) of sending entity 
(default: -). For example, ${processid} or ${processname}
* `msgid` ([Layout](https://github.com/NLog/NLog/wiki/Layouts)): [message type identifier](http://tools.ietf.org/html/rfc5424#section-6.2.7) (numeric or alphanumeric) of sending entity 
(default: -). For example, ${callsite}
* `structureddata` ([Layout](https://github.com/NLog/NLog/wiki/Layouts)): [additional data](http://tools.ietf.org/html/rfc5424#section-6.3) of sending entity (default: -).
For example, [thread@12345 id="${threadid}" name="${threadname}"][mydata2@12345 num="1" code="mycode"]

#### Log message body
This target supports the standard NLog 
[layout](https://github.com/NLog/NLog/wiki/Layouts) directive to modify
the log message body. The Syslog packet elements are not affected.



## NLog
See more about NLog at: [http://nlog-project.org](http://nlog-project.org)



<br />
<br />



# Test bench
 1. `[HOST]` Download VirtualBox and Vagrant and install them
 2. `[HOST]` Download an [Ubuntu Vagrant box](http://cloud-images.ubuntu.com/vagrant/)
 3. `[HOST]` Create a Vagrantfile in the same folder of the downloaded box
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
 5. `[HOST]` Add the box to the list
    ```shell
    vagrant box add .\ubuntuvagrant.box --name 'ubuntu/trusty64'
    vagrant box list
    ```
 6. `[HOST]` Start the VM
    ```shell
    vagrant up
    ```
 7. `[HOST]` Connect to the VM with SSH
 8. `[GUEST]` Switch to the root user
    ```shell
    su
    ```
 9. `[GUEST]` Uncomment the following `/etc/rsyslog.conf` lines:
    ```
    #$ModLoad imudp
    #$UDPServerRun 514
    ```
    ```
    #$ModLoad imtcp
    #$InputTCPServerRun 514
    ```
10. `[GUEST]` Add the following `/etc/rsyslog.d/50-default.conf` line under the `user.*` one (prefixing a path with the minus sign omits flushing after every log event)
    ```
    local4.*                        /var/log/local4.log
    ```
11. `[GUEST]` Restart Syslog service
    ```shell
    service rsyslog restart
    ```
12. `[HOST]` Restart the VM
    ```shell
    vagrant reload
    ```
13. `[GUEST]` Make sure rsyslog is running
    ```shell
    ps -A | grep rsyslog
    ```
14. `[GUEST]` Check the rsyslog configuration
    ```shell
    rsyslogd -N1
    ```
15. `[GUEST]` Check the Linux system log for rsyslog errors
    ```shell
    cat /var/log/syslog | grep rsyslog
    ```
16. `[GUEST]` Perform a local test
    ```shell
    logger --server 127.0.0.1 --port 514 --priority local4.error "TCP local test"
    logger --server 127.0.0.1 --port 514 --priority local4.warning --udp "UDP local test"
    tail -3 /var/log/syslog
    tail -3 /var/log/local4.log
    ```
17. `[GUEST]` Prepare for a remote test
    ```shell
    tail -f /var/log/syslog
    ```
18. `[HOST]` Perform a remote test
    ```shell
    telnet 127.0.0.1 514
    ```
19. `[HOST]` Perform a remote test with the NLog target (configuring it to use the Local4 facility)



<br />
<br />



# Syslog message format
Messages are sent using the format defined in
[RFC 3164](http://www.ietf.org/rfc/rfc3164.txt) or
[RFC 5424](http://tools.ietf.org/html/rfc5424).


### RFC 3164
There are no set requirements on the contents of the Syslog message: the payload of any Syslog message must be considered to be a valid syslog message.
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
SYSLOG MESSAGE = HEADER SPACE STRUCTUREDDATA (SPACE MSG)

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

STRUCTUREDDATA = NILVALUE or 1 or more SDELEMENT
    SDELEMENT = [ SDID (one or more SPACE PARAMNAME = " PARAMVALUE ") ]
        SDID
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
        PARAMNAME
            1 to 32 SAFEPRINTUSASCII
        PARAMVALUE
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