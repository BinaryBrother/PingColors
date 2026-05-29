using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PingColors
{
    internal class CLI
    {
        /// <summary>
        /// A simple method to get the value of a command line argument. It checks for duplicates and missing values, throwing exceptions if necessary.
        /// Accepts an array of two possible commands.
        /// </summary>
        /// <param name="pCommands"></param>
        /// <param name="pArgs"></param>
        /// <returns>Argument Value</returns>
        /// <exception cref="Exception"></exception>
        public static string GetValue(string[] pCommands, string[] pArgs)
        {
            int Iteration = 0;
            string Result = "";
            for (int i = 0; i < pCommands.Length; i++)
            {
                for (int j = 0; j < pArgs.Length; j++)
                {
                    if (pCommands[i] == pArgs[j])
                    {
                        Iteration++;
                        if (Iteration > 1) { throw new Exception($"Duplicate arguments used!"); }
                        if (j + 1 < pArgs.Length)
                        {
                            Result = pArgs[j + 1];
                        }
                        else
                        {
                            throw new Exception($"Value for {pCommands[i]} does not exist!");
                        }
                    }
                }
            }
            return Result;
        }
        internal void HandleArgs(ref int warningResponseTime, ref int criticalResponseTime, ref int timeout, ref IPAddress host, ref bool speedMode, string[] args)
        {
            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "--warning":
                        case "-w":
                            if (i + 1 < args.Length) { warningResponseTime = int.Parse(args[++i]); }
                            break;

                        case "--critical":
                        case "-c":
                            if (i + 1 < args.Length) { criticalResponseTime = int.Parse(args[++i]); }
                            break;
                        case "--timeout":
                        case "-t":
                            if (i + 1 < args.Length) { timeout = int.Parse(args[++i]); }
                            break;
                        case "--speedmode":
                        case "-s":
                            speedMode = true;
                            break;
                        case "--help":
                        case "-h":
                            Custom.ShowHelp();
                            break;
                        default:
                            if (Uri.CheckHostName(args[i]) != UriHostNameType.Unknown)
                            {
                                host = Dns.GetHostAddresses(args[i]).First(address => address.AddressFamily == AddressFamily.InterNetwork);
                                //Host = args[i]; // Assign the valid hostname or IP address
                            }
                            else
                            {
                                Custom.Error($"Unknown argument: {args[i]}");
                                Custom.ShowHelp();
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Custom.Error($"Error parsing arguments: {ex.Message}");
                Custom.ShowHelp();
            }
        }
    }
}
