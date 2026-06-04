using System.Net;
using System.Net.Sockets;

namespace PingColors
{
    internal class CLI
    {
        public void HandleArgs(ref int warningResponseTime, ref int criticalResponseTime, ref int timeout, ref IPAddress host, ref bool speedMode, string[] args)
        {
            try
            {
                if (args.Length == 0) { Custom.ShowHelp(); }
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
                        case "/?":
                            Custom.ShowHelp();
                            break;
                        default:
                            if (Uri.CheckHostName(args[i]) != UriHostNameType.Unknown)
                            {
                                host = Dns.GetHostAddresses(args[i]).First(address => address.AddressFamily == AddressFamily.InterNetwork);
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
