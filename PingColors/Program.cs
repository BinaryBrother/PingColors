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
            IPAddress? host = null;
            
            CLI commandLineInterface = new();
            Custom customMethods = new();

            Console.CancelKeyPress += delegate { Console.ResetColor(); Console.WriteLine("Exiting..."); };
            Console.Title = "PingColors";
            if (args.Length == 0) { Custom.ShowHelp(); }
            commandLineInterface.HandleArgs(ref warningResponseTime, ref criticalResponseTime, ref timeout, ref host, ref speedMode, args);
            customMethods.ErrorChecking(warningResponseTime, criticalResponseTime, timeout, host);
            customMethods.Ping(host, timeout, warningResponseTime, criticalResponseTime, speedMode);
        }
    }
}
