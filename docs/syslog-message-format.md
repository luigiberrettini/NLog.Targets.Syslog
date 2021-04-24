# Syslog message format

Messages are built using the format defined in [RFC 3164](https://tools.ietf.org/html/rfc3164) or [RFC 5424](https://tools.ietf.org/html/rfc5424).

They are then sent using the protocol defined in [RFC 5426](https://tools.ietf.org/html/rfc5426) or [RFC 6587](https://tools.ietf.org/html/rfc6587) or [RFC 5425](https://tools.ietf.org/html/rfc5425).



## RFC 3164

There are no set requirements on the contents of the Syslog message: the payload of any Syslog message must be considered to be a valid Syslog message.
It is, however, recommended for the Syslog message to have all the parts described here.


### Conventions

 - `SPACE`: the ASCII value `dec 32` / `hex 20`
 - `PRINTUSASCII`: ASCII values in the range `dec [33, 126]` / `hex [21, 7E]`


### Message parts

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


### Examples

 - `<34>Oct 11 00:14:05 mymachine su: 'su root' failed for lonvick on /dev/pts/8`
 - `<13>Feb  5 17:32:18 10.0.0.99 myTag Use the BFG!`



## RFC 5424


### Conventions

 - `(section)`: brackets are used to indicate that a section is optional
 - `NILVALUE`: the hyphen i.e. ASCII value dec 45 / hex 2D
 - `SPACE`: the ASCII value `dec 32` / `hex 20`
 - `PRINTUSASCII`: ASCII values in the range `dec [33, 126]` / `hex [21, 7E]`
 - `SAFEPRINTUSASCII`: PRINTUSASCII except `=`, `]`, `"`


### Message parts

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


### Examples

 - `<34>1 2003-10-11T22:14:15.003Z mymachine.example.com su - ID47 - BOM'su root' failed for lonvick on /dev/pts/8`
 - `<165>1 2003-08-24T05:14:15.000003-07:00 192.0.2.1 myproc 8710 - - %% It's time to make the do-nuts.`
 - `<165>1 2003-10-11T22:14:15.003Z mymachine.example.com evntslog - ID47 [exampleSDID@32473 iut="3" eventSource="Application" eventID="1011"] BOMAn application event log entry...`
 - `<165>1 2003-10-11T22:14:15.003Z mymachine.example.com evntslog - ID47 [exampleSDID@32473 iut="3" eventSource="Application" eventID="1011"][examplePriority@32473 class="high"]`