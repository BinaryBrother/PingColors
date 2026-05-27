using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PingColors
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int warningResponseTime = 80; // Default warning threshold in milliseconds
            int criticalResponseTime = 200; // Default critical threshold in milliseconds
            int timeout = 5000; // Default timeout for ping in milliseconds
            bool speedMode = false; // Default speed mode is off
            IPAddress? Host = null;

            Console.CancelKeyPress += delegate { Console.ResetColor(); Console.WriteLine("Exiting..."); };
            Console.Title = "Ping Colors";
            if (args.Length == 0) { Custom.ShowHelp(); }
            CLI.HandleArgs(ref warningResponseTime, ref criticalResponseTime, ref timeout, ref Host, ref speedMode, args);
            Custom.ErrorChecking(warningResponseTime, criticalResponseTime, timeout, Host);
            Custom.Ping(Host, timeout, warningResponseTime, criticalResponseTime, speedMode);
        }
    }
}
