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