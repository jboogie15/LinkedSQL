# LinkedSQL
Tool developed for OSEP exam to aid in exploitation of MS SQL Servers and lateral movement inside of Active Directory via SQL Servers.

#Usage

- Using `/instance` flag will identify what privileges the user is currently running in and identify if the SQL Server has any links to other SQL Servers.

`LinkedSQL.exe /instance sql1`

- After identify any links, the user can supply `/linkedinstance` along with the `/checkrpc` flag to check if RPC Out (disabled by default) is enabled. If RPC Out is disabled, LinkedSQL will attempt to enable on the linked server.

`LinkedSQL.exe /instance sql1 /linkedinstance sql2 /checkrpc`

- With RPC Out enabled, using `/command` followed by a command will go through the process of enabling xp_cmdshell and running the command supplied by the user. For better opsec, the user can supply `/opsec` as well to have xp_cmdshell disabled after having their command executed.

`LinkedSQL.exe /instance sql1 /linkedinstance sql2 /command whoami /opsec`

- In the case you have credentials of a user and want to interact with SQL server with that user, you can run with `/uname` and `/pwd` with the user's username and password respectively. To query the SQL server, supply `/query`.

![image](https://user-images.githubusercontent.com/67240643/165570367-7fdf30d5-08b1-40b7-95c1-e78ad1eec3cd.png)
